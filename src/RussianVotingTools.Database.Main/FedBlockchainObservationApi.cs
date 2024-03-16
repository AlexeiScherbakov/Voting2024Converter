using System.Diagnostics;

using NHibernate.Linq;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class FedBlockchainObservationApi
		:IFedBlockchainObservationApi
	{
		private readonly static ActivitySource _activitySource = new ActivitySource("FedBlockchainObservationApi");

		private readonly MainDatabaseNHibernateService _nhibernateService;

		public FedBlockchainObservationApi(MainDatabaseNHibernateService nhibernateService)
		{
			_nhibernateService = nhibernateService;
		}

		private static string MakeDatabaseName(Guid uid)
		{
			return "fed_obs_db_" + uid.ToString("N");
		}

		public async Task<long> CreateAsync(long electionObservationId, FedBlockchainObservationData data)
		{
			using var activity = _activitySource.StartActivity("Create");
			using var session = _nhibernateService.SessionFactory.OpenSession();

			var databaseName = MakeDatabaseName(data.Uid);
			var query = session.CreateSQLQuery("CREATE DATABASE " + databaseName);
			var test = await query.UniqueResultAsync();

			using var tr = session.BeginTransaction();
			DbBlockchainObservation dbObject = new();
			dbObject.ElectionObservation = session.Load<DbElectionObservation>(electionObservationId);
			BlockchainObservationConverter.CopyToDatabaseObject(data, dbObject);
			await session.SaveAsync(dbObject);
			await tr.CommitAsync();
			return dbObject.ID;
		}

		public async Task<bool> DeleteAsync(long electionObservationId, bool deleteAllData)
		{
			using var activity = _activitySource.StartActivity("Delete");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			var dbObject = await session.Query<DbBlockchainObservation>().Where(x => x.ID == electionObservationId).SingleOrDefaultAsync();
			if (dbObject is null)
			{
				return false;
			}
			return false;
		}

		public async Task<FedBlockchainObservationStorageData[]> GetAllAsync()
		{
			using var activity = _activitySource.StartActivity("GetAll");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			session.DefaultReadOnly = true;

			var dbObjects = await session.Query<DbBlockchainObservation>().ToListAsync().ConfigureAwait(false);

			return dbObjects.ConvertToArray(BlockchainObservationConverter.ToStorageData);
		}

		public async Task<bool> UpdateAsync(long electionObservationId, FedBlockchainObservationData data)
		{
			using var activity = _activitySource.StartActivity("Update");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();
			var dbObject = await session.Query<DbBlockchainObservation>().Where(x => x.ID == electionObservationId).SingleOrDefaultAsync();
			if (dbObject is null)
			{
				return false;
			}
			BlockchainObservationConverter.CopyToDatabaseObject(data, dbObject);
			await session.UpdateAsync(dbObject);
			await tr.CommitAsync();
			return true;
		}
	}
}
