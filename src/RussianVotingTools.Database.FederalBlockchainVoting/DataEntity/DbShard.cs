using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using NHibernate.Linq.ReWriters;

namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	public class DbShard
		: DbIdObject
	{
		private string _name;
		private int _logicalNumber;
		private ISet<DbBlock> _blocks = new HashSet<DbBlock>();

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual int LogicalNumber
		{
			get { return _logicalNumber; }
			set { _logicalNumber = value; }
		}

		public virtual ISet<DbBlock> Blocks
		{
			get { return _blocks; }
			protected set { _blocks = value; }
		}
	}
}
