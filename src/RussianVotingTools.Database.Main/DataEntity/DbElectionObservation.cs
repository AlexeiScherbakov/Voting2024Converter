using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace RussianVotingTools.Database.Main.DataEntity
{
	public class DbElectionObservation
		: DbIdObject
	{
		private DbElectionTimeline _electionTimeline;
		private string _name;
		private ISet<DbBlockchainObservation> _blockchainObservations = new HashSet<DbBlockchainObservation>();

		public virtual DbElectionTimeline ElectionTimeline
		{
			get { return _electionTimeline; }
			set { _electionTimeline = value; }
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual ISet<DbBlockchainObservation> BlockchainObservations
		{
			get { return _blockchainObservations; }
			protected set { _blockchainObservations = value; }
		}
	}
}
