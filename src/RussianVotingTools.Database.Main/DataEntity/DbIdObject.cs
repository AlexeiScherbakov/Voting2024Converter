namespace RussianVotingTools.Database.Main.DataEntity
{
	public abstract class DbIdObject
	{
		private long _id;

		public virtual long ID
		{
			get { return _id; }
			set { _id = value; }
		}
	}
}
