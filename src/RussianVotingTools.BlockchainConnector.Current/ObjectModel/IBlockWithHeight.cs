namespace RussianVotingTools.BlockchainConnector.ObjectModel
{
	public interface IBlockWithHeight
	{
		long Height { get; }
		long Timestamp { get; }

		Google.Protobuf.ByteString BlockSignature { get; }
	}
}


