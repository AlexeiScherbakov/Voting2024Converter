using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.Json;

using NHibernate.Linq;

using RussianVotingTools.Database.FederalBlockchainVoting;
using RussianVotingTools.Database.Main;
using RussianVotingTools.Database.Watcher;

using Voting2021.FilesUtils;

using WatcherDumpImporter.WatcherImport;

namespace WatcherDumpImporter
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Task.Run(() => Execute("config.json")).Wait();
		}

		private static async Task Execute(string fileName)
		{
			var jsonConfig = File.ReadAllText(fileName);

			var importConfig = JsonSerializer.Deserialize<ImportConfiguration>(jsonConfig);

			var connection = await MainDatabaseConnection.OpenPostgresConnectionAsync(
				importConfig.DatabaseConnection.Server,
				importConfig.DatabaseConnection.Port,
				importConfig.DatabaseConnection.UserName,
				importConfig.DatabaseConnection.Password);

			var watch = Stopwatch.StartNew();

			WatcherImporter importer = new(connection)
			{
				ImportBlockchainInformation = true
			};


			await importer.ProcessShardsData(importConfig.Shards);

			Console.WriteLine("Import completed {0}", watch.Elapsed);
		}
	}
}
