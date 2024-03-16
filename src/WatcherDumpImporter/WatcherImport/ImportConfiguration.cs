using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace WatcherDumpImporter.WatcherImport
{
	public class ImportConfiguration
	{
		[JsonPropertyName("databaseConnection")]
		public PostgresDatabaseConnection DatabaseConnection { get; set; }

		[JsonPropertyName("shards")]
		public ShardImportInfo[] Shards { get; set; }
	}


	public sealed class PostgresDatabaseConnection
	{
		[JsonPropertyName("server")]
		public string Server { get; set; }

		[JsonPropertyName("port")]
		public int Port { get; set; }

		[JsonPropertyName("userName")]
		public string UserName { get; set; }

		[JsonPropertyName("password")]
		public string Password { get; set; }
	}
}
