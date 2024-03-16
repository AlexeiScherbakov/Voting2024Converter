namespace WatcherDumpImporter.WatcherImport
{
	public abstract class WatcherFilesProcessor
	{
		public abstract Task ProcessShardsData(ShardImportInfo[] shards);
	}
}
