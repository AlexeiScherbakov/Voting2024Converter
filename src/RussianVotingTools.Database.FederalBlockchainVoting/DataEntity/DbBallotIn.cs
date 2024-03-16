using RussianVotingTools.Database.FederalBlockchainVoting.DataEntity;

namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	/// <summary>
	/// Получение бюллетеня урной (отправка бюллетеня избирателем)
	/// </summary>
	public class DbBallotIn
		: DbIdObject
	{
		public virtual DbVoting Voting { get; set; }

		public virtual DateTime Time { get; set; }
	}
}
