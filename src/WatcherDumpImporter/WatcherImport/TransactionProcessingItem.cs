namespace WatcherDumpImporter.WatcherImport
{
	/// <summary>
	/// Задача по импорту транзакции
	/// </summary>
	public sealed class TransactionProcessingItem
	{
		public long ShardId;
		public byte[] TransactionBody;
		public long Offset;
		public bool Rollback;
		public int Height;
		public Dictionary<string, string> Properties;
		public WavesEnterprise.Transaction Transaction;
	}
}
