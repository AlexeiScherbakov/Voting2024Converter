namespace RussianVotingTools.Database.Main.Abstractions
{
	public interface IFedBlockchainObservationApi
	{
		Task<long> CreateAsync(long electionObservationId, FedBlockchainObservationData data);

		Task<bool> UpdateAsync(long electionObservationId, FedBlockchainObservationData data);

		Task<bool> DeleteAsync(long electionObservationId, bool deleteAllData);

		Task<FedBlockchainObservationStorageData[]> GetAllAsync();
	}
}
