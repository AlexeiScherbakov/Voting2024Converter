

using System.Diagnostics.Contracts;
using System.Reflection.Metadata;

using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;


namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	internal sealed class FederalBlockchainVotingNHibernateService
		: IDisposable
	{
		private readonly static HbmMapping _mapping;

		static FederalBlockchainVotingNHibernateService()
		{
			_mapping = CreateMapping();
		}

		private static HbmMapping CreateMapping()
		{
			var mapper = new ModelMapper();

			mapper.Class<DbShard>(clazz =>
			{
				clazz.Table(TableName.Shard);
				
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned);
				});

				clazz.Property(x => x.LogicalNumber, map =>
				{
					map.Column(ColumnName.LogicalNumber);
					map.NotNullable(true);
				});

				clazz.Property(x => x.Name, map =>
				{
					map.Column(ColumnName.Name);
					map.NotNullable(true);
				});

				clazz.Set(x => x.Blocks, map =>
				{
					map.Table(TableName.Block);
					map.Key(x => x.Column(ColumnName.ShardID));
					map.Inverse(true);
				}, relation => relation.OneToMany());
			});

			mapper.Class<DbBlock>(clazz =>
			{
				clazz.Table(TableName.Block);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						//g.Params(new Dictionary<string, object>()
						//{
						//	{ "sequence","seq_blockchain_block" },
						//	{ "table","seq_blockchain_block" },
						//	{ "column","lastid" }
						//});
					});
				});
				clazz.ManyToOne(x => x.Shard, map =>
				{
					map.Column(ColumnName.ShardID);
					map.Index("idx_" + TableName.Block + "_" + ColumnName.ShardID);
					map.NotNullable(true);
				});
				clazz.Property(x => x.Height, map =>
				{
					map.Column(ColumnName.Height);
					map.Index("idx_" + TableName.Block + "_" + ColumnName.Height);
					map.NotNullable(true);
				});
				clazz.Property(x => x.Signature, map =>
				{
					map.Column(ColumnName.Signature);
					map.NotNullable(true);
				});
				clazz.Property(x => x.Timestamp, map =>
				{
					map.Column(ColumnName.Timestamp);
					map.NotNullable(true);
				});
				clazz.Property(x => x.DeletedAt, map =>
				{
					map.Column(ColumnName.DeletedAt);
					map.NotNullable(false);
				});
				clazz.Set(x => x.Transactions, map =>
				{
					map.Table(TableName.BlockchainTransaction);
					map.Key(x => x.Column(ColumnName.BlockID));
					map.Inverse(true);
				}, relation => relation.OneToMany());
			});

			mapper.Class<DbBlockchainTransaction>(clazz =>
			{
				clazz.Table(TableName.BlockchainTransaction);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						//g.Params(new Dictionary<string, object>()
						//{
						//	{ "sequence","seq_blockchain_tx" },
						//	{ "table","seq_blockchain_tx" },
						//	{ "column","lastid" }
						//});
					});
				});

				clazz.ManyToOne(x => x.Block, map =>
				{
					map.Column(ColumnName.BlockID);
					map.Index("idx_" + TableName.BlockchainTransaction + "_" + ColumnName.BlockID);
					map.NotNullable(true);
				});

				clazz.Property(x => x.TxId, map =>
				{
					map.Column("TxId");
					map.Index("idx_" + TableName.BlockchainTransaction + "_" + "TxId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.NestedTxId, map =>
				{
					map.Column("NestedTxId");
					map.Index("idx_" + TableName.BlockchainTransaction + "_" + "NestedTxId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.ContractId, map =>
				{
					map.Column("ContractId");
					map.Index("idx_" + TableName.BlockchainTransaction + "_" + "ContractId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Body, map =>
				{
					map.Column("Body");
					map.NotNullable(false);
				});

				clazz.Property(x => x.DeletedAt, map =>
				{
					map.Column(ColumnName.DeletedAt);
					map.NotNullable(false);
				});
			});

			#region Голосование

			mapper.Class<DbVotingContract>(clazz =>
			{
				clazz.Table(TableName.VotingContract);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned);
				});

				clazz.Property(x => x.ContractId, map =>
				{
					map.Column("ContractId");
					map.Index("idx_" + TableName.VotingContract + "_" + "ContractId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.ExecuteTxId, map =>
				{
					map.Column("ExecuteTxId");
					map.Index("idx_" + TableName.VotingContract + "_" + "ExecuteTxId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Timestamp, map =>
				{
					map.Column("Timestamp");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Name, map =>
				{
					map.Column("Name");
					map.NotNullable(false);
				});

				clazz.Property(x => x.Image, map =>
				{
					map.Column("Image");
					map.NotNullable(false);
				});

				clazz.Property(x => x.ImageHash, map =>
				{
					map.Column("ImageHash");
					map.NotNullable(false);
				});

				clazz.Property(x => x.PollId, map =>
				{
					map.Column("PollId");
					map.NotNullable(false);
				});

				clazz.Property(x => x.CommissionId, map =>
				{
					map.Column("CommissionId");
					map.NotNullable(false);
				});

				clazz.Property(x => x.BulletinHash, map =>
				{
					map.Column("BulletinHash");
					map.NotNullable(false);
				});

				clazz.Property(x => x.IsRevoteBlocked, map =>
				{
					map.Column("IsRevoteBlocked");
					map.NotNullable(false);
				});
			});

			mapper.Class<DbVoting>(clazz =>
			{
				clazz.Table(TableName.Voting);
				clazz.DynamicUpdate(true);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned);
				});

				clazz.ManyToOne(x => x.VotingContract, map =>
				{
					map.Column("VotingContractID");
					map.Index("idx_" + TableName.Voting + "_" + "VotingContractID");
					map.NotNullable(true);
				});

				clazz.Property(x => x.TimeCreate, map =>
				{
					map.Column("TimeCreate");
					map.NotNullable(true);
				});

				clazz.Property(x => x.TimeStart, map =>
				{
					map.Column("TimeStart");
					map.NotNullable(false);
				});

				clazz.Property(x => x.TimeEnd, map =>
				{
					map.Column("TimeEnd");
					map.NotNullable(false);
				});
			});

			#endregion

			mapper.Class<DbVoterListEventBlockchainTransaction>(clazz =>
			{
				clazz.Table(TableName.VoterListEventTransaction);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned);
				});

				clazz.Property(x => x.Timestamp, map =>
				{
					map.Column("Timestamp");
					map.NotNullable(true);
				});

				clazz.Property(x => x.ExecuteTxId, map =>
				{
					map.Column("ExecuteTxId");
					map.Index("idx_" + TableName.VoterListEventTransaction + "_" + "ExecuteTxId");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Operation, map =>
				{
					map.Column("Operation");
					map.NotNullable(true);
				});

				clazz.Property(x => x.PrimaryUikRegionCode, map =>
				{
					map.Column("PrimaryUikRegionCode");
					map.NotNullable(true);
				});

				clazz.Property(x => x.PrimaryUikNumber, map =>
				{
					map.Column("PrimaryUikNumber");
					map.NotNullable(true);
				});

				clazz.ManyToOne(x => x.VotingContract, map =>
				{
					map.Column("VotingContractID");
					map.Index("idx_" + TableName.VoterListEventTransaction + "_" + "VotingContractID");
					map.NotNullable(true);
				});
			});

			mapper.Class<DbVoterListEvent>(clazz =>
			{
				clazz.Table(TableName.VoterListEvent);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						g.Params(new Dictionary<string, object>()
						{
							{ "sequence","seq_voterlistevent" }
						});
					});
				});

				clazz.ManyToOne(x => x.Voting, map =>
				{
					map.Column("VotingID");
					map.Index("idx_" + TableName.VoterListEvent + "_" + "VotingID");
					map.NotNullable(true);
				});

				clazz.ManyToOne(x => x.EventTransaction, map =>
				{
					map.Column("EventTransactionID");
					map.Index("idx_" + TableName.VoterListEvent + "_" + "EventTransactionID");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Operation, map =>
				{
					map.Column("Operation");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Time, map =>
				{
					map.Column("Time");
					map.NotNullable(true);
				});

				clazz.Property(x => x.UikNumber, map =>
				{
					map.Column("Uik");
					map.NotNullable(true);
				});

				clazz.Property(x => x.UikRegionCode, map =>
				{
					map.Column("UikRegion");
					map.NotNullable(true);
				});

			});

			mapper.Class<DbVoterListEventItem>(clazz =>
			{
				clazz.Table(TableName.VoterListEventItem);

				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						g.Params(new Dictionary<string, object>()
						{
							{ "sequence","seq_voterlisteventitem" }
						});
					});
				});

				clazz.ManyToOne(x => x.Voter, map =>
				{
					map.Column("VoterID");
					map.Index("idx_" + TableName.VoterListEventItem + "_" + "VoterID");
					map.NotNullable(true);
				});

				clazz.ManyToOne(x => x.Event, map =>
				{
					map.Column("EventID");
					map.Index("idx_" + TableName.VoterListEventItem + "_" + "EventID");
					map.NotNullable(true);
				});

				clazz.Property(x => x.Sum, map =>
				{
					map.Column("Sum");
					map.NotNullable(true);
				});
			});

			mapper.Class<DbVoter>(clazz =>
			{
				clazz.Table(TableName.Voter);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						g.Params(new Dictionary<string, object>()
						{
							{ "sequence","seq_voter" }
						});
					});
				});

				clazz.Property(x => x.Hash, map =>
				{
					map.Column("Hash");
					map.NotNullable(true);
				});
			});

			mapper.Class<DbBallotOut>(clazz =>
			{
				clazz.Table(TableName.BallotOut);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						g.Params(new Dictionary<string, object>()
						{
							{ "sequence","seq_ballot_out" }
						});
					});
				});

				clazz.Property(x => x.Time, map =>
				{
					map.Column("Time");
					map.NotNullable(true);
				});

				clazz.Property(x => x.RegionCode, map =>
				{
					map.Column("RegionCode");
					map.NotNullable(false);
				});

				clazz.Property(x => x.UikNumber, map =>
				{
					map.Column("UikNumber");
					map.NotNullable(true);
				});

				clazz.ManyToOne(x => x.Voter, map =>
				{
					map.Column("VoterID");
					map.Index("idx_" + TableName.BallotOut + "_" + "VoterID");
					map.NotNullable(false);
				});

				clazz.ManyToOne(x => x.Voting, map =>
				{
					map.Column("VotingID");
					map.Index("idx_" + TableName.BallotOut + "_" + "VotingID");
					map.NotNullable(true);
				});
			});

			mapper.Class<DbBallotIn>(clazz =>
			{
				clazz.Table(TableName.BallotIn);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Assigned, g =>
					{
						g.Params(new Dictionary<string, object>()
						{
							{ "sequence","seq_ballot_in" }
						});
					});
				});

				clazz.Property(x => x.Time, map =>
				{
					map.Column("Time");
					map.NotNullable(true);
				});

				clazz.ManyToOne(x => x.Voting, map =>
				{
					map.Column("VotingID");
					map.Index("idx_" + TableName.BallotIn + "_" + "VotingID");
					map.NotNullable(true);
				});
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		private readonly ISessionFactory _sessionFactory;

		private FederalBlockchainVotingNHibernateService(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory;
		}

		public void Dispose()
		{
			_sessionFactory.Dispose();
		}

		public ISessionFactory SessionFactory
		{
			get { return _sessionFactory; }
		}

		public static FederalBlockchainVotingNHibernateService CreatePostgres(string connectionString,bool exportSchema)
		{
			Configuration cfg = new();
			cfg.DataBaseIntegration(
				db =>
				{
					db.MaximumDepthOfOuterJoinFetching = 6;
					db.ConnectionString = connectionString;
					db.ConnectionProvider<DriverConnectionProvider>();
					db.Driver<NHibernate.Driver.NpgsqlDriver>();
					db.Dialect<NHibernate.Dialect.PostgreSQL83Dialect>();
					db.BatchSize = 1000;
					db.Batcher<GenericBatchingBatcherFactory>();
				})
				.Proxy(x => x.ProxyFactoryFactory<NHibernate.Bytecode.StaticProxyFactoryFactory>());
			cfg.AddMapping(_mapping);

			if (exportSchema)
			{
				// создаем БД
				var export = new SchemaExport(cfg);
				export.SetDelimiter(";");
				export.Execute(false, true, false);
			}
			else
			{
				// обновляем схему
				var update = new SchemaUpdate(cfg);
				update.Execute(false, true);
			}

			var sessionFactory = cfg.BuildSessionFactory();
			return new FederalBlockchainVotingNHibernateService(sessionFactory);
		}
	}
}
