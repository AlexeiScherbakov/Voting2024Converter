namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	public class DbBlock
		: DbIdObject
	{
		private DbShard _shard;
		private int _height;
		private byte[] _signature;
		private DateTime _timestamp;
		private DateTime? _deletedAt;
		private ISet<DbBlockchainTransaction> _transactions = new HashSet<DbBlockchainTransaction>();

		public virtual DbShard Shard
		{
			get { return _shard; }
			set { _shard = value; }
		}

		public virtual int Height
		{
			get { return _height; }
			set { _height = value; }
		}

		public virtual byte[] Signature
		{
			get { return _signature; }
			set { _signature = value; }
		}

		public virtual DateTime Timestamp
		{
			get { return _timestamp; }
			set { _timestamp = value; }
		}

		public virtual DateTime? DeletedAt
		{
			get { return _deletedAt; }
			set { _deletedAt = value; }
		}

		public virtual ISet<DbBlockchainTransaction> Transactions
		{
			get { return _transactions; }
			protected set { _transactions = value; }
		}
	}
}
