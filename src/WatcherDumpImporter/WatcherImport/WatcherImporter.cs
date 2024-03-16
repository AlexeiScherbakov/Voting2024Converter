using System.Data.SQLite;
using System.Text.Json.Nodes;

using NHibernate;
using NHibernate.Linq;

using RussianVotingTools.BlockchainConnector;
using RussianVotingTools.BlockchainConnector.Abstractions;
using RussianVotingTools.Database.FederalBlockchainVoting;
using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;
using RussianVotingTools.Database.Main;
using RussianVotingTools.Database.Watcher;

using Voting2021.FilesUtils;

namespace WatcherDumpImporter.WatcherImport
{
	public sealed class WatcherImporter
		: WatcherFilesProcessor
	{
		private readonly MainDatabaseConnection _mainDatabaseConnection;

		private bool _importTransationBodies;
		private bool _importBlockchainInformation;
		private bool _importParsedTransactions;


		public WatcherImporter(MainDatabaseConnection mainDatabaseConnection)
		{
			_mainDatabaseConnection = mainDatabaseConnection;
		}

		public bool ImportTransactionBodies
		{
			get { return _importTransationBodies; }
			set { _importTransationBodies = value; }
		}

		public bool ImportBlockchainInformation
		{
			get { return _importBlockchainInformation; }
			set { _importBlockchainInformation = value; }
		}

		public bool ImportParsedTransactions
		{
			get { return _importParsedTransactions; }
			set { _importParsedTransactions = value; }
		}

		public override async Task ProcessShardsData(ShardImportInfo[] shards)
		{
			var existing = await _mainDatabaseConnection.ElectionTimeline.GetAllAsync();

			long timelineId = 0;
			var timeline = existing.Where(x => x.Data.Name == "Autoimport").SingleOrDefault();
			if (timeline is null)
			{
				timelineId = await _mainDatabaseConnection.ElectionTimeline.CreateAsync(new()
				{
					Name = "Autoimport",
					PlannedEndTime = DateTime.Now,
					PlannedStartTime = DateTime.Now,
				});
			}
			else
			{
				timelineId = timeline.Id;
			}

			var observationId = await _mainDatabaseConnection.ElectionObservation.CreateAsync(timelineId, new()
			{
				Name = "Import " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"),
			});
			var databaseUid = Guid.NewGuid();
			var fedBlockchainObservationId = await _mainDatabaseConnection.BlockchainObservation.CreateAsync(observationId, new()
			{
				Name = "Blockchain import " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"),
				Uid = databaseUid
			});

			await ImportToNewDatabase(shards, databaseUid);
		}

		public async Task ImportToNewDatabase(ShardImportInfo[] shards, Guid databaseUid)
		{
			FederalBlockchainVotingConnection connection = await FederalBlockchainVotingConnection.CreateDatabaseAsync(_mainDatabaseConnection.ConnectionString, databaseUid);

			IdGenerator idGenerator = new();

			foreach (var shard in shards)
			{
				Console.WriteLine("Импорт шарда {0}", shard.StateDb);
				long dbShardId = 0;

				if (_importBlockchainInformation)
				{
					using (var session = connection.SessionFactory.OpenSession())
					using (var tr = session.BeginTransaction())
					{
						DbShard dbShard = new()
						{
							ID = idGenerator.GetNextId<DbShard>(),
							LogicalNumber = shard.LogicalNumber,
							Name = shard.Name
						};
						await session.SaveAsync(dbShard);
						await tr.CommitAsync();
						dbShardId = dbShard.ID;
					}

					SQLiteConnectionStringBuilder builder = new()
					{
						BinaryGUID = true,
						DataSource = shard.StateDb
					};

					using var stateStore = StateStore.Sqlite(builder.ToString(), false);

					// создаем записи блоков
					int maxHeight = 0;
					Console.WriteLine("Загрузка блоков");
					using (var stateStoreSession = stateStore.OpenSession())
					{
						stateStoreSession.DefaultReadOnly = true;

						var blocks = await stateStoreSession.Query<Block>().ToListAsync();
						using (var session = connection.SessionFactory.OpenStatelessSession())
						using (var tr = session.BeginTransaction())
						{
							var dbShard = session.Get<DbShard>(dbShardId);
							foreach (var block in blocks)
							{
								DbBlock dbBlock = new()
								{
									ID = idGenerator.GetNextId<DbBlock>(),
									Shard = dbShard,
									Height = block.Height,
									Signature = block.Signature,
									Timestamp = block.Timestamp,
									DeletedAt = block.DeletedAt != DateTime.MinValue ? block.DeletedAt : null
								};
								if (dbBlock.Height > maxHeight)
								{
									maxHeight = dbBlock.Height;
								}

								await session.InsertAsync(dbBlock);
							}
							await tr.CommitAsync();
						}
					}
					// идем по высоте блоков от 0 до maxHeight по 100 штук и импортируем оттуда транзакции
					int steps = maxHeight / 100 + 2;

					for (int i = 0; i < steps; i++)
					{
						bool isLast = i >= (steps - 3);
						try
						{
							int min = i * 100;
							int max = min + 100;
							Console.WriteLine("Загрузка транзакций блоков [{0};{1})", min, max);
							using var session = connection.SessionFactory.OpenStatelessSession();
							using var stateStoreSession = stateStore.OpenSession();
							stateStoreSession.DefaultReadOnly = true;
							var dbBlock = await session.Query<DbBlock>().Where(x => x.Shard.ID == dbShardId && x.Height >= min && x.Height < max).ToListAsync();

							var heightToBlock = dbBlock.ToDictionary(x => x.Height, x => x);

							var transactions = await stateStoreSession.Query<Tx>()
								.Where(x => x.Height >= min && x.Height < max)
								.ToListAsync();

							using (var tr = session.BeginTransaction())
							{
								foreach (var transaction in transactions)
								{
									DbBlockchainTransaction dbBlockchainTransaction = new()
									{
										ID = idGenerator.GetNextId<DbBlockchainTransaction>(),
										Block = heightToBlock[transaction.Height],
										ContractId = transaction.ContractId,
										NestedTxId = transaction.NestedTxId,
										TxId = transaction.TxId,
										Body = null,
										DeletedAt = transaction.DeletedAt != DateTime.MinValue ? transaction.DeletedAt : null
									};
									await session.InsertAsync(dbBlockchainTransaction);
								}

								await tr.CommitAsync();
							}
						}
						catch (Exception e)
						{
							// Для скопированного дампа ок, если последние 1-3 блока ошибочных (чтобы копировать из файлового менеджера)
							if (!isLast)
							{
								throw e;
							}
						}

					}
				}
				if (_importParsedTransactions)
				{
					Console.WriteLine("Обработка транзакций по типам");
					using var transactionFileReader = new TransactionFileReader(shard.TransactionLog);
					using var importer = new ParallelImporter(connection, idGenerator)
					{
						ImportTransactionBodies = _importBlockchainInformation && _importTransationBodies,
					};
					while (!transactionFileReader.Eof)
					{
						try
						{
							var record = transactionFileReader.ReadRecord();

							TransactionProcessingItem task = new()
							{
								Offset = record.offset,
								TransactionBody = record.binaryTransaction,
								Properties = record.properties,
								ShardId = dbShardId
							};
							await importer.EnqueueTransactionForProcessing(task);
						}
						catch (Exception)
						{

						}
					}
					await importer.EndAndWaitForCompletion();
				}
			}
		}


		private sealed class ParallelImporter
			: BatchWatcherTransactionLogProcessor
		{
			private readonly FederalBlockchainVotingConnection _connection;

			private bool _importTransationBodies;

			private ImportCache _importCache = new();

			private IdGenerator _idGenerator;

			private IStatelessSession _statelessSession;

			public ParallelImporter(FederalBlockchainVotingConnection connection,IdGenerator idGenerator)
				:base (1000)
			{
				_connection = connection;
				_idGenerator = idGenerator;

				_statelessSession = _connection.SessionFactory.OpenStatelessSession();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				_statelessSession.Close();
			}

			#region Блок настроек
			public bool ImportTransactionBodies
			{
				get { return _importTransationBodies; }
				set { _importTransationBodies = value; }
			}
			#endregion


			protected override async Task TransactionBatchProcessingAction(TransactionProcessingItem[] tasks)
			{
				try
				{
					ImportTransactionsUnsafe(tasks);
				}
				catch (Exception e)
				{
					Console.WriteLine("Error");
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}

			private void ImportTransactionsUnsafe(TransactionProcessingItem[] tasks)
			{
				Console.WriteLine("Importing {0}", tasks.Length);
				var heights = tasks.GroupBy(x => x.ShardId)
					.Select(x => new
					{
						x.Key,
						Items = x.Select(y => y.Height).Distinct()
					}).ToDictionary(x => x.Key, x => x.Items.ToArray());


				
				using var tr = _statelessSession.BeginTransaction();

				foreach (var task in tasks)
				{
					var operationType = task.Transaction.GetOperationType();
					switch (operationType)
					{
						case "Executed CreateContractTransaction voting-contract":
							CreateVotingTransaction(_statelessSession, task);
							break;
						case "Executed CallContractTransaction addVotersList":
							AddVotersList(_statelessSession, task);
							break;
						case "Executed CallContractTransaction addMainKey":
							AddMainKey(_statelessSession, task);
							break;
						case "Executed CallContractTransaction startVoting":
							StartVoting(_statelessSession, task);
							break;
						case "Executed CallContractTransaction blindSigIssue":
							BlindSigIssue(_statelessSession, task);
							break;
						case "Executed CallContractTransaction vote":
							Vote(_statelessSession, task);
							break;
						case "Executed CallContractTransaction removeFromVotersList":
							RemoveFromVotersList(_statelessSession, task);
							break;
						case "Executed CallContractTransaction finishVoting":
							FinishVoting(_statelessSession, task);
							break;
						case "Executed CallContractTransaction decryption":
							break;
						case "Executed CallContractTransaction results":
							break;
						case "Executed CallContractTransaction commissionDecryption":
							break;
						default:
							break;
					}
				}

				tr.Commit();
			}

			#region Обработка транзакций
			private void CreateVotingTransaction(IStatelessSession session, TransactionProcessingItem task)
			{
				var create = task.Transaction.ExecutedContractTransaction.Tx.CreateContractTransaction;
				var executeId = task.Transaction.ExecutedContractTransaction.Id.ToByteArray();
				var createId = create.Id.ToByteArray();
				var result = task.Transaction.ExecutedContractTransaction.Results;
				var votingBase = result.Where(x => x.Key == "VOTING_BASE").Single().StringValue;
				string? bulletinHash = null;
				string? pollId = null;
				bool? isRevoteBlocked = null;
				string? commissionId = null;
				{
					var json = (JsonObject)JsonNode.Parse(votingBase);
					if (json.TryGetPropertyValue("isRevoteBlocked", out var jsonNode))
					{
						isRevoteBlocked = jsonNode.GetValue<bool>();
					}
					if (json.TryGetPropertyValue("bulletinHash", out jsonNode))
					{
						var base64 = jsonNode.ToString();
						bulletinHash = base64;
					}
					if (json.TryGetPropertyValue("pollId", out jsonNode))
					{
						pollId = jsonNode.ToString();
					}
					if (json.TryGetPropertyValue("commissionId", out jsonNode))
					{
						commissionId = jsonNode.ToString();
					}
				}

				var timestamp = task.Transaction.ExecutedContractTransaction.Timestamp;
				var image = create.Image;
				var imageHash = create.ImageHash;
				var contractName = create.ContractName;

				DbVotingContract dbVotingContract = new()
				{
					ID = _idGenerator.GetNextId<DbVotingContract>(),
					ContractId = createId,
					ExecuteTxId = executeId,
					Name = contractName,
					Image = image,
					ImageHash = imageHash,
					Timestamp = timestamp,
					BulletinHash = bulletinHash,
					IsRevoteBlocked = isRevoteBlocked,
					CommissionId = commissionId,
					PollId = pollId,
				};

				session.Insert(dbVotingContract);

				var time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
				DbVoting dbVoting = new()
				{
					ID = _idGenerator.GetNextId<DbVoting>(),
					VotingContract = dbVotingContract,
					TimeCreate = time.ToOffset(TimeSpan.Zero).DateTime,
				};

				session.Insert(dbVoting);
				_importCache.CacheDbVoting(dbVoting);
			}

			private void AddVotersList(IStatelessSession session, TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;
				var executeId = task.Transaction.ExecutedContractTransaction.Id.ToByteArray();
				var operation = data.Params.Where(x => x.Key == "operation").Single().StringValue;
				var contractId = data.Params.Where(x => x.Key == "contractId").Single().StringValue;
				var contractIdBytes = Base58.DecodePlain(contractId);

				var key = data.Results[0].Key;

				var json = JsonNode.Parse(data.Results[0].StringValue);
				var userIdHashes = json["userIdHashes"];
				var primaryUikRegionCode = json["primaryUikRegionCode"].ToString();
				var primaryUikNumber = json["primaryUikNumber"].AsValue().GetValue<int>();

				(var votingCotract, var voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(contractIdBytes);

				DbVoterListEventBlockchainTransaction dbEventTransaction = new()
				{
					ID = _idGenerator.GetNextId<DbVoterListEventBlockchainTransaction>(),
					ExecuteTxId = executeId,
					Operation = operation,
					VotingContract = votingCotract,
					Timestamp = task.Transaction.ExecutedContractTransaction.Timestamp,
					PrimaryUikNumber = primaryUikNumber,
					PrimaryUikRegionCode = primaryUikRegionCode
				};

				session.Insert(dbEventTransaction);

				var opEnum = GetVoterListOperation(operation);

				var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).ToOffset(TimeSpan.Zero).DateTime;

				DbVoterListEvent dbVoterListEvent = new()
				{
					ID = _idGenerator.GetNextId<DbVoterListEvent>(),
					EventTransaction = dbEventTransaction,
					Operation = opEnum,
					UikNumber = primaryUikNumber,
					UikRegionCode = int.TryParse(primaryUikRegionCode, out var uikRegionCode) ? uikRegionCode : -1,
					Voting = voting,
					Time = dateTime
				};
				session.Insert(dbVoterListEvent);

				foreach (var userIdHashJson in userIdHashes.AsArray())
				{
					var userIdHash = userIdHashJson.ToString();
					if (!_importCache.TryGetDbVoterByHash(userIdHash, out var dbVoter))
					{
						dbVoter = new()
						{
							ID = _idGenerator.GetNextId<DbVoter>(),
							Hash = userIdHash
						};
						session.Insert(dbVoter);
						_importCache.CacheDbVoter(dbVoter);
					}
					int sum = GetSumForOperation(opEnum);
					DbVoterListEventItem dbEvent = new()
					{
						ID = _idGenerator.GetNextId<DbVoterListEventItem>(),
						Event = dbVoterListEvent,
						Sum = sum,
						Voter = dbVoter
					};
					session.Insert(dbEvent);
				}
			}

			private void AddMainKey(IStatelessSession session, TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;

				var executeId = task.Transaction.ExecutedContractTransaction.Id.ToByteArray();
				var operation = data.Params.Where(x => x.Key == "operation").Single().StringValue;
				var mainKey = data.Params.Where(x => x.Key == "mainKey").Single().StringValue;
				var commissionKey = data.Params.Where(x => x.Key == "commissionKey").Single().StringValue;
				var dkgKey = data.Params.Where(x => x.Key == "dkgKey").Single().StringValue;

				var mainKey2 = data.Results.Where(x => x.Key == "MAIN_KEY").Single().StringValue;
				var commissionKey2 = data.Results.Where(x => x.Key == "COMMISSION_KEY").Single().StringValue;
				var dkgKey2 = data.Results.Where(x => x.Key == "DKG_KEY").Single().StringValue;
			}

			private void StartVoting(IStatelessSession session,TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;

				var dateStartParams = data.Params.Where(x => x.Key == "dateStart").SingleOrDefault()?.StringValue;
				var votingBaseString = data.Results.Where(x => x.Key == "VOTING_BASE").Single().StringValue;
				var json = JsonNode.Parse(votingBaseString);
				var dateStartResult = json["dateStart"].AsValue().ToString();

				(_, var voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(data.ContractID);
				voting.TimeStart = DateTime.Parse(dateStartResult);
				session.Update(voting);
			}

			private void BlindSigIssue(IStatelessSession session,TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;
				var dataJson = data.Params.Where(x => x.Key == "data").Single().StringValue;
				string primaryUikRegionCode = data.Params.Where(x => x.Key == "primaryUikRegionCode").Single().StringValue;
				long primaryUikNumber = data.Params.Where(x => x.Key == "primaryUikNumber").Single().IntValue;
				var json = JsonNode.Parse(dataJson).AsArray();
				var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).ToOffset(TimeSpan.Zero).DateTime;
				foreach (var userJson in json)
				{
					var userId = (string?) userJson["userId"];
					DbVoter? dbVoter = null;
					if (_importCache.TryGetDbVoterByHash(userId,out dbVoter))
					{
						
					}

					(_, DbVoting voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(data.ContractID);
					var maskedSig = (string?) userJson["maskedSig"];
					DbBallotOut dbBallotOut = new DbBallotOut()
					{
						ID = _idGenerator.GetNextId<DbBallotOut>(),
						Time = dateTime,
						Voter = dbVoter,
						Voting = voting,
						RegionCode = primaryUikRegionCode,
						UikNumber = (int) primaryUikNumber
					};
					session.Insert(dbBallotOut);
				}
			}

			private void Vote(IStatelessSession session, TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;
				var vote = data.Params.Where(x => x.Key == "vote").Single().BinaryValue.ToArray();
				var blindSig = data.Params.Where(x => x.Key == "blindSig").Single().BinaryValue.ToArray();

				var dateTime=DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).ToOffset(TimeSpan.Zero).DateTime;

				(_, DbVoting voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(data.ContractID);
				DbBallotIn dbBallotIn = new()
				{
					ID = _idGenerator.GetNextId<DbBallotIn>(),
					Voting = voting,
					Time = dateTime
				};
				try
				{
					session.Insert(dbBallotIn);
				}
				catch(Exception e)
				{
					throw;
				}
				
			}

			private static int GetSumForOperation(VoterListOperationType type)
			{
				switch (type)
				{
					case VoterListOperationType.Add:
					case VoterListOperationType.Restore:
						return +1;
					case VoterListOperationType.Remove:
						return -1;
					default:
						return 0;
				}
			}

			private static VoterListOperationType GetVoterListOperation(string operationName)
			{
				switch (operationName)
				{
					case "addVotersList":
						return VoterListOperationType.Add;
					case "removeFromVotersList":
						return VoterListOperationType.Remove;
					default:
						return VoterListOperationType.None;
				}
			}

			private void RemoveFromVotersList(IStatelessSession session, TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;
				var executeId = task.Transaction.ExecutedContractTransaction.Id.ToByteArray();
				var operation = data.Params.Where(x => x.Key == "operation").Single().StringValue;
				var contractIdBytes = data.ContractID;

				var key = data.Results[0].Key;

				var json = JsonNode.Parse(data.Results[0].StringValue);
				var userIdHashes = json["userIdHashes"];
				var primaryUikRegionCode = json["primaryUikRegionCode"].ToString();
				var primaryUikNumber = json["primaryUikNumber"].AsValue().GetValue<int>();

				(var votingCotract, var voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(contractIdBytes);

				DbVoterListEventBlockchainTransaction dbEventTransaction = new()
				{
					ID = _idGenerator.GetNextId<DbVoterListEventBlockchainTransaction>(),
					ExecuteTxId = executeId,
					Operation = operation,
					VotingContract = votingCotract,
					Timestamp = task.Transaction.ExecutedContractTransaction.Timestamp,
					PrimaryUikNumber = primaryUikNumber,
					PrimaryUikRegionCode = primaryUikRegionCode
				};

				session.Insert(dbEventTransaction);

				var opEnum = GetVoterListOperation(operation);

				DbVoterListEvent dbVoterListEvent = new()
				{
					ID = _idGenerator.GetNextId<DbVoterListEvent>(),
					EventTransaction = dbEventTransaction,
					Operation = opEnum,
					UikNumber = primaryUikNumber,
					UikRegionCode = int.TryParse(primaryUikRegionCode, out var uikRegionCode) ? uikRegionCode : -1,
					Voting = voting,
				};
				session.Insert(dbVoterListEvent);

				foreach (var userIdHashJson in userIdHashes.AsArray())
				{
					var userIdHash = userIdHashJson.ToString();
					if (!_importCache.TryGetDbVoterByHash(userIdHash, out var dbVoter))
					{
						dbVoter = new()
						{
							ID = _idGenerator.GetNextId<DbVoter>(),
							Hash = userIdHash
						};
						session.Insert(dbVoter);
						_importCache.CacheDbVoter(dbVoter);
					}
					int sum = GetSumForOperation(opEnum);
					DbVoterListEventItem dbEvent = new()
					{
						ID = _idGenerator.GetNextId<DbVoterListEventItem>(),
						Event = dbVoterListEvent,
						Sum = sum,
						Voter = dbVoter
					};
					session.Insert(dbEvent);
				}

			}

			private void FinishVoting(IStatelessSession session,TransactionProcessingItem task)
			{
				ExecuteCallContractTransactionFields data = task.Transaction;
				var votingBaseString = data.Results.Where(x => x.Key == "VOTING_BASE").Single().StringValue;
				var json = JsonNode.Parse(votingBaseString);
				var dateEndResult = json["dateEnd"].AsValue().ToString();

				(_, DbVoting voting) = _importCache.GetVotingDatabaseEntitiesByBlockchainContractId(data.ContractID);
				voting.TimeEnd = DateTime.Parse(dateEndResult);
				session.Update(voting);
			}
			#endregion

		}

		
	}
}
