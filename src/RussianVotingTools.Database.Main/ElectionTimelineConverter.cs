using System.Diagnostics.CodeAnalysis;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class ElectionTimelineConverter
		: IDatabaseObjectToDataObjectConverter<DbElectionTimeline, ElectionTimelineData, ElectionTimelineStorageData>
	{
		private ElectionTimelineConverter()
		{

		}

		/// <inheritdoc/>
		public static void CopyToDatabaseObject(ElectionTimelineData source, DbElectionTimeline destination)
		{
			destination.Name = source.Name;
			destination.PlannedStartTime = source.PlannedStartTime.ToOffset(TimeSpan.Zero).DateTime;
			destination.PlannedEndTime = source.PlannedEndTime.ToOffset(TimeSpan.Zero).DateTime;
			destination.StartTime = source.StartTime?.ToOffset(TimeSpan.Zero).DateTime;
			destination.EndTime = source.EndTime?.ToOffset(TimeSpan.Zero).DateTime;
		}

		/// <inheritdoc/>
		public static void CopyToDataObject(DbElectionTimeline source, ElectionTimelineData destination)
		{
			destination.Name = source.Name;
			destination.PlannedStartTime = source.PlannedStartTime;
			destination.PlannedEndTime = source.PlannedEndTime;
			destination.StartTime = source.StartTime;
			destination.EndTime = source.EndTime;
		}

		/// <inheritdoc/>
		public static ElectionTimelineData ToData(DbElectionTimeline dbObject)
		{
			ElectionTimelineData ret = new();
			CopyToDataObject(dbObject, ret);
			return ret;
		}

		/// <inheritdoc/>
		[return: NotNullIfNotNull("dbObject")]
		public static ElectionTimelineStorageData? ToStorageData(DbElectionTimeline? dbObject)
		{
			if (dbObject is null)
			{
				return null;
			}
			ElectionTimelineStorageData ret = new()
			{
				Id = dbObject.ID,
				Data = ToData(dbObject)
			};
			return ret;
		}
	}
}
