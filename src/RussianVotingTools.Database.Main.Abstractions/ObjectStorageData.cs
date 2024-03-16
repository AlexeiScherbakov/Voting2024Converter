namespace RussianVotingTools.Database.Main.Abstractions
{
	public class ObjectStorageData<T>
	{
		public ObjectStorageData()
		{

		}

		public ObjectStorageData(long id, T obj)
		{
			Id = id;
			Data = obj;
		}

		public long Id { get; set; }

		public T Data { get; set; }
	}
}
