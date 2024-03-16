namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	public class DbBlockchainTransaction
		: DbIdObject
	{
		private DbBlock _block;
		private byte[] _txId;
		private byte[] _nestedTxId;
		private byte[] _contractId;
		private DateTime? _deletedAt;
		private byte[]? _bytes;

		public virtual DbBlock Block
		{
			get { return _block; }
			set { _block = value; }
		}

		public virtual byte[] TxId
		{
			get { return _txId; }
			set { _txId = value; }
		}

		public virtual byte[] NestedTxId
		{
			get { return _nestedTxId; }
			set { _nestedTxId = value; }
		}

		public virtual byte[] ContractId
		{
			get { return _contractId; }
			set { _contractId = value; }
		}

		public virtual DateTime? DeletedAt
		{
			get { return _deletedAt; }
			set { _deletedAt = value; }
		}

		public virtual byte[]? Body
		{
			get { return _bytes; }
			set { _bytes = value; }
		}
	}
}
