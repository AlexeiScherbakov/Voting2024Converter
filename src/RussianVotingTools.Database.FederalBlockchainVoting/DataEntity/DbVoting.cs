namespace RussianVotingTools.Database.FederalBlockchainVoting.DataEntity
{
	public class DbVoting
		: DbIdObject
	{
		public virtual DateTime TimeCreate { get; set; }
		public virtual DateTime? TimeStart { get; set; }
		public virtual DateTime? TimeEnd { get; set; }
		public virtual DbVotingContract VotingContract { get; set; }

	}
}
