namespace RussianVotingTools.Database.Main.Abstractions
{
	public class ElectionObservationStorageData
		:ObjectStorageData<ElectionObservationData>
	{
		public long ElectionTimelineId { get; set; }
	}
}
