using System.Diagnostics.CodeAnalysis;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class ElectionObservationConverter
		: IDatabaseObjectToDataObjectConverter<DbElectionObservation, ElectionObservationData, ElectionObservationStorageData>
	{
		private ElectionObservationConverter()
		{

		}

		public static void CopyToDatabaseObject(ElectionObservationData source, DbElectionObservation destination)
		{
			destination.Name = source.Name;
		}

		public static void CopyToDataObject(DbElectionObservation source, ElectionObservationData destination)
		{
			destination.Name = source.Name;
		}

		public static ElectionObservationData ToData(DbElectionObservation? dbObject)
		{
			ElectionObservationData ret = new();
			CopyToDataObject(dbObject, ret);
			return ret;
		}

		[return: NotNullIfNotNull("dbObject")]
		public static ElectionObservationStorageData? ToStorageData(DbElectionObservation dbObject)
		{
			if (dbObject is null)
			{
				return null;
			}
			ElectionObservationStorageData ret = new()
			{
				Id = dbObject.ID,
				ElectionTimelineId = dbObject.ElectionTimeline.ID,
				Data = ToData(dbObject)
			};
			return ret;
		}
	}
}
