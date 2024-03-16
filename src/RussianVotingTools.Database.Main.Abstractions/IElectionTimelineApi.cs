namespace RussianVotingTools.Database.Main.Abstractions
{
	public interface IElectionTimelineApi
	{
		Task<long> CreateAsync(ElectionTimelineData electionTimelineData);
		Task<bool> UpdateAsync(long electionTimelineId, ElectionTimelineData electionTimelineData);

		Task<bool> DeleteAsync(long electionTimelineId);

		Task<ElectionTimelineStorageData[]> GetAllAsync();
	}
}
