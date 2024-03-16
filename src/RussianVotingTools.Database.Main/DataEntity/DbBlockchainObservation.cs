namespace RussianVotingTools.Database.Main.DataEntity
{
	public class DbBlockchainObservation
		: DbIdObject
	{
		private DbElectionObservation _electionObservation;
		private Guid _uid;
		private string _name;

		public virtual DbElectionObservation ElectionObservation
		{
			get { return _electionObservation; }
			set { _electionObservation = value; }
		}

		public virtual Guid Uid
		{
			get { return _uid; }
			set { _uid = value; }
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}
	}
}
