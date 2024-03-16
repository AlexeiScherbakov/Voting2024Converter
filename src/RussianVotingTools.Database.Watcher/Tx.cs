namespace RussianVotingTools.Database.Watcher
{
	public class Tx
	{
		public virtual long Id { get; set; }

		public virtual int Height { get; set; }

		public virtual byte[] TxId { get; set; }

		public virtual byte[] NestedTxId { get; set; }

		public virtual byte[] ContractId { get; set; }

		public virtual DateTime? DeletedAt { get; set; }
	}
}
