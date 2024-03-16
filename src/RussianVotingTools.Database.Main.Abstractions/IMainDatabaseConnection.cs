namespace RussianVotingTools.Database.Main.Abstractions
{
	public interface IMainDatabaseConnection
	{
		IElectionTimelineApi ElectionTimeline { get; }

		IElectionObservationApi ElectionObservation { get; }

		IFedBlockchainObservationApi BlockchainObservation { get; }
	}
}
