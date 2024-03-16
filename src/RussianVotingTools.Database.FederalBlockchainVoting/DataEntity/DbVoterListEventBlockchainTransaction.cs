using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;

namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	public class DbVoterListEventBlockchainTransaction
		: DbIdObject
	{
		public virtual byte[] ExecuteTxId { get; set; }
		public virtual string Operation { get; set; }
		public virtual DbVotingContract VotingContract { get; set; }

		public virtual long Timestamp { get; set; }

		public virtual string PrimaryUikRegionCode { get; set; }

		public virtual int PrimaryUikNumber { get; set; }
	}

	public class DbVoterListEvent
		: DbIdObject
	{
		public virtual DbVoting Voting { get; set; }
		public virtual VoterListOperationType Operation { get; set; }
		public virtual DbVoterListEventBlockchainTransaction EventTransaction { get; set; }


		public virtual DateTime Time { get; set; }
		public virtual int UikRegionCode { get; set; }
		public virtual int UikNumber { get; set; }
	}

	public class DbVoterListEventItem
		: DbIdObject
	{
		public virtual DbVoterListEvent Event { get; set; }
		public virtual DbVoter Voter { get; set; }
		public virtual int Sum { get; set; }
	}
}
