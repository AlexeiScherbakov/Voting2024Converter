using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;
using RussianVotingTools.BlockchainConnector.Abstractions;
using RussianVotingTools.Database.FederalBlockchainVoting;

namespace WatcherDumpImporter.WatcherImport
{
	/// <summary>
	/// Кэш для потокового импорта транзакций
	/// </summary>
	internal sealed class ImportCache
		:IDisposable
	{
		Dictionary<byte[], long> _contractsDatabaseIdByBlockchainId=new(ByteStringEqualityComparer.Instance);
		Dictionary<long, DbVotingContract> _dbVotingContracts = new();
		Dictionary<long, DbVoting> _dbVotings = new();
		Dictionary<long, long> _contractIdToVotingId = new();

		Dictionary<string, long> _voterDatabaseIdByHashDictionary = new();
		Dictionary<long, DbVoter> _dbVoters = new Dictionary<long, DbVoter>();

		public ImportCache()
		{
		}

		public void Dispose()
		{
			_contractsDatabaseIdByBlockchainId.Clear();
			_voterDatabaseIdByHashDictionary.Clear();
			_contractIdToVotingId.Clear();
		}

		public (DbVotingContract dbVotingCotract,DbVoting voting) GetVotingDatabaseEntitiesByBlockchainContractId(byte[] blockchainId)
		{
			var contractId = _contractsDatabaseIdByBlockchainId[blockchainId];
			var votingContract = _dbVotingContracts[contractId];
			var votingId = _contractIdToVotingId[contractId];
			var voting = _dbVotings[votingId];
			return (votingContract, voting);
		}

		public void CacheDbVoting(DbVoting dbVoting)
		{
			if (dbVoting.ID == 0)
			{
				throw new InvalidOperationException("Голосование должно быть добавлено в базу");
			}
			if ((dbVoting.VotingContract is null)||(dbVoting.VotingContract.ID==0))
			{
				throw new InvalidOperationException("Голосование должно быть слинковано с контрактом");
			}
			_contractsDatabaseIdByBlockchainId.Add(dbVoting.VotingContract.ContractId, dbVoting.VotingContract.ID);
			_contractIdToVotingId.Add(dbVoting.VotingContract.ID, dbVoting.ID);
			_dbVotingContracts.Add(dbVoting.VotingContract.ID, dbVoting.VotingContract);
			_dbVotings.Add(dbVoting.ID, dbVoting);
		}

		public void CacheDbVoter(DbVoter dbVoter)
		{
			if (dbVoter.ID == 0)
			{
				throw new InvalidOperationException("Избиратель должен быть добавлен в базу");
			}
			if (dbVoter.Hash is null)
			{
				throw new InvalidOperationException("Хеш избирателя должен быть заполнен");
			}
			_voterDatabaseIdByHashDictionary.Add(dbVoter.Hash, dbVoter.ID);
			_dbVoters.Add(dbVoter.ID, dbVoter);
		}

		public bool TryGetDbVoterByHash(string hash, out DbVoter dbVoter)
		{
			if (_voterDatabaseIdByHashDictionary.TryGetValue(hash, out var dbVoterId))
			{
				dbVoter = _dbVoters[dbVoterId];
				return true;
			}
			dbVoter = null;
			return false;
		}

		
	}
}
