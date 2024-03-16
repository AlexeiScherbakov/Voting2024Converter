using CommandLine;

namespace WatcherDumpImporter
{
	public class CommandLineOptions
	{
		[Option("config", Required = false, HelpText = "Файл конфигурации импорта")]
		public string ConfigFile { get; set; }
	}
}
