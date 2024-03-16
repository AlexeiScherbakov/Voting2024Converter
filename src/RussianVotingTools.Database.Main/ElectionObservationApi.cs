using System.Diagnostics;
using System.Reflection.PortableExecutable;

using NHibernate.Linq;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class ElectionObservationApi
		: IElectionObservationApi
	{
		private readonly static ActivitySource _activitySource = new ActivitySource("ElectionObservationApi");

		private readonly MainDatabaseNHibernateService _nhibernateService;

		public ElectionObservationApi(MainDatabaseNHibernateService nhibernateService)
		{
			_nhibernateService = nhibernateService;
		}

		public async Task<long> CreateAsync(long electionTimelineId, ElectionObservationData electionObservationData)
		{
			using var activity = _activitySource.StartActivity("Create");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			DbElectionObservation dbElectionObservation = new();
			ElectionObservationConverter.CopyToDatabaseObject(electionObservationData, dbElectionObservation);
			dbElectionObservation.ElectionTimeline = session.Get<DbElectionTimeline>(electionTimelineId);
			await session.SaveAsync(dbElectionObservation).ConfigureAwait(false);
			
			await tr.CommitAsync().ConfigureAwait(false);

			return dbElectionObservation.ID;
		}

		public async Task<bool> DeleteAsync(long electionObservationId, bool deleteAllData)
		{
			using var activity = _activitySource.StartActivity("Delete");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			var dbElectionObservation = session.Query<DbElectionObservation>()
				.Where(x => x.ID == electionObservationId)
				.SingleOrDefault();
			if (dbElectionObservation is null)
			{
				return false;
			}
			if (dbElectionObservation.BlockchainObservations.Count > 0)
			{
				return false;
			}
			await session.DeleteAsync(dbElectionObservation).ConfigureAwait(false);
			await tr.CommitAsync().ConfigureAwait(false);

			return true;
		}

		public async Task<ElectionObservationStorageData[]> GetAllAsync()
		{
			using var activity = _activitySource.StartActivity("GetAll");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			session.DefaultReadOnly = true;

			var dbObjects = await session.Query<DbElectionObservation>().ToListAsync().ConfigureAwait(false);

			var ret = dbObjects.ConvertToArray(ElectionObservationConverter.ToStorageData);
			return ret;
		}

		public async Task<ElectionObservationStorageData[]> GetForTimelineAsync(int electionTimelineId)
		{
			using var activity = _activitySource.StartActivity("GetAll");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			session.DefaultReadOnly = true;

			var dbObjects = await session.Query<DbElectionObservation>().Where(x => x.ElectionTimeline.ID == electionTimelineId).ToListAsync().ConfigureAwait(false);

			var ret = dbObjects.ConvertToArray(ElectionObservationConverter.ToStorageData);
			return ret;
		}

		public async Task<bool> UpdateAsync(long electionObservationId, ElectionObservationData electionObservationData)
		{
			using var activity = _activitySource.StartActivity("Update");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			var dbElectionObservation = session.Query<DbElectionObservation>()
				.Where(x => x.ID == electionObservationId)
				.SingleOrDefault();
			if (dbElectionObservation is null)
			{
				return false;
			}

			ElectionObservationConverter.CopyToDatabaseObject(electionObservationData, dbElectionObservation);

			await session.UpdateAsync(dbElectionObservation).ConfigureAwait(false);
			await tr.CommitAsync().ConfigureAwait(false);

			return true;
		}
	}
}
