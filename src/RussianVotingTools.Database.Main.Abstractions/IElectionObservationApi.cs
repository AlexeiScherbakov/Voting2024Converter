namespace RussianVotingTools.Database.Main.Abstractions
{
	public interface IElectionObservationApi
	{
		Task<long> CreateAsync(long electionTimelineId, ElectionObservationData electionObservationData);

		Task<bool> UpdateAsync(long electionObservationId, ElectionObservationData electionObservationData);

		Task<bool> DeleteAsync(long electionObservationId, bool deleteAllData);

		Task<ElectionObservationStorageData[]> GetAllAsync();

		Task<ElectionObservationStorageData[]> GetForTimelineAsync(int electionTimelineId);
	}
}
