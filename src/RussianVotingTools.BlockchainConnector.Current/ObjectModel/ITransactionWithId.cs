namespace RussianVotingTools.BlockchainConnector.ObjectModel
{
	public interface ITransactionWithId
	{
		Google.Protobuf.ByteString Id { get; }
		long Timestamp { get; }
	}
}
