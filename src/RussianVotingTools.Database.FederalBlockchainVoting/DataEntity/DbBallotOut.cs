using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;

namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	/// <summary>
	/// Выдача бюллетеня системой (получение бюллетеня избирателем)
	/// </summary>
	public class DbBallotOut
		: DbIdObject
	{
		public virtual DbVoting Voting { get; set; }
		public virtual DbVoter? Voter { get; set; }

		public virtual DateTime Time { get; set; }

		public virtual int UikNumber { get; set; }
		public virtual string RegionCode { get; set; }
	}
}
