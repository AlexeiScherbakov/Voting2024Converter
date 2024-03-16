using System.Text.Json.Serialization;

namespace WatcherDumpImporter.WatcherImport
{
	public class ShardImportInfo
	{
		[JsonPropertyName("logicalNumber")]
		public int LogicalNumber { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("stateDb")]
		public string StateDb { get; set; }

		[JsonPropertyName("transactionLog")]
		public string TransactionLog { get; set; }

	}
}
