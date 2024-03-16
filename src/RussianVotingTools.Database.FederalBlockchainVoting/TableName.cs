namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	internal static class TableName
	{
		public const string BallotIn = "BallotIn";
		public const string BallotOut = "BallotOut";
		public const string Shard = "Shard";
		public const string Block = "Block";
		public const string BlockchainTransaction = "BlockchainTransaction";
		public const string VotingContract = "VotingContract";
		public const string Voting = "Voting";
		public const string VoterListEvent = "VoterListEvent";
		public const string VoterListEventItem = "VoterListEventItem";
		public const string VoterListEventTransaction = "VoterListEventTransaction";
		public const string Voter = "Voter";

		public static IEnumerable<string> GetAllTables()
		{
			// ИМЕНА УПОРЯДОЧЕНЫ ПО ЛЕСУ ЗАВИСИМОСТЕЙ ОТ ЛИСТОВ К КОРНЯМ !!!
			yield return BallotIn;
			yield return BallotOut;

			yield return VoterListEventItem;
			yield return VoterListEvent;
			yield return VoterListEventTransaction;
			yield return Voter;

			yield return Voting;
			yield return VotingContract;

			yield return BlockchainTransaction;
			yield return Block;
			yield return Shard;
		}
	}
}
