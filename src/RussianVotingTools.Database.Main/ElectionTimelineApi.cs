using System.Diagnostics;

using NHibernate.Linq;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class ElectionTimelineApi
		: IElectionTimelineApi
	{
		private readonly static ActivitySource _activitySource = new ActivitySource("ElectionTimelineApi");

		private readonly MainDatabaseNHibernateService _nhibernateService;

		public ElectionTimelineApi(MainDatabaseNHibernateService nhibernateService)
		{
			_nhibernateService = nhibernateService;
		}

		public async Task<long> CreateAsync(ElectionTimelineData electionTimelineData)
		{
			using var activity = _activitySource.StartActivity("Create");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			DbElectionTimeline dbElectionTimeline = new();

			ElectionTimelineConverter.CopyToDatabaseObject(electionTimelineData, dbElectionTimeline);

			await session.SaveAsync(dbElectionTimeline).ConfigureAwait(false);

			await tr.CommitAsync().ConfigureAwait(false);

			return dbElectionTimeline.ID;
		}

		public async Task<bool> DeleteAsync(long electionTimelineId)
		{
			using var activity = _activitySource.StartActivity("Delete");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			var dbElectionTimeline =await session.Query<DbElectionTimeline>()
				.Where(x => x.ID == electionTimelineId)
				.SingleOrDefaultAsync().ConfigureAwait(false);

			if (dbElectionTimeline is null)
			{
				return false;
			}

			var nestedCount = dbElectionTimeline.ElectionObservations.Count;
			if (nestedCount > 0)
			{
				return false;
			}

			await session.DeleteAsync(dbElectionTimeline).ConfigureAwait(false);

			await tr.CommitAsync().ConfigureAwait(false);

			return true;
		}

		public async Task<ElectionTimelineStorageData[]> GetAllAsync()
		{
			using var activity = _activitySource.StartActivity("GetAll");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			session.DefaultReadOnly = true;

			var dbObjects = await session.Query<DbElectionTimeline>()
				.ToListAsync();

			var ret = dbObjects.ConvertToArray(ElectionTimelineConverter.ToStorageData);
			return ret;
		}

		public async Task<bool> UpdateAsync(long electionTimelineId, ElectionTimelineData electionTimelineData)
		{
			using var activity = _activitySource.StartActivity("Update");

			using var session = _nhibernateService.SessionFactory.OpenSession();
			using var tr = session.BeginTransaction();

			var dbElectionTimeline = await session.Query<DbElectionTimeline>()
				.Where(x => x.ID == electionTimelineId)
				.SingleOrDefaultAsync().ConfigureAwait(false);

			if (dbElectionTimeline is null)
			{
				return false;
			}
			ElectionTimelineConverter.CopyToDatabaseObject(electionTimelineData, dbElectionTimeline);

			await session.UpdateAsync(dbElectionTimeline).ConfigureAwait(false);

			await tr.CommitAsync().ConfigureAwait(false);

			return true;
		}
	}
}
