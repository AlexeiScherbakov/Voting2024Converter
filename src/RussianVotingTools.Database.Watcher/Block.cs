namespace RussianVotingTools.Database.Watcher
{
	public class Block
	{
		public virtual long Id { get; set; }

		public virtual int Height { get; set; }

		public virtual byte[] Signature { get; set; }

		public virtual DateTime Timestamp { get; set; }

		public virtual DateTime? DeletedAt { get; set; }


		public virtual long StartOffset { get; set; }
		public virtual long EndOffset { get; set; }
	}
}
