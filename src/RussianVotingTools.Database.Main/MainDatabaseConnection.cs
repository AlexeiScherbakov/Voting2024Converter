using System.Runtime.InteropServices;

using RussianVotingTools.Database.Main.Abstractions;

namespace RussianVotingTools.Database.Main
{
	public sealed class MainDatabaseConnection
		: IMainDatabaseConnection
	{
		private MainDatabaseNHibernateService _mainDatabaseService;
		private ElectionTimelineApi _electionTimelineApi;
		private ElectionObservationApi _electionObservationApi;
		private FedBlockchainObservationApi _blockchainObservationApi;
		private MainDatabaseConnection(MainDatabaseNHibernateService mainDatabaseService)
		{
			_mainDatabaseService = mainDatabaseService;
			_electionTimelineApi = new(_mainDatabaseService);
			_electionObservationApi = new(_mainDatabaseService);
			_blockchainObservationApi = new(_mainDatabaseService);
		}
		
		public string ConnectionString
		{
			get { return _mainDatabaseService.ConnectionString; }
		}


		public IElectionTimelineApi ElectionTimeline
		{
			get { return _electionTimelineApi; }
		}

		public IElectionObservationApi ElectionObservation
		{
			get { return _electionObservationApi; }
		}

		public IFedBlockchainObservationApi BlockchainObservation
		{
			get { return _blockchainObservationApi; }
		}

		private static MainDatabaseConnection OpenPostgresConnection(string server, int port, string userName, string password)
		{
			Npgsql.NpgsqlConnectionStringBuilder builder = new()
			{
				Host = server,
				Port = port,
				Username = userName,
				Password = password,
				Database = "main_db"
			};
			var connectionString = builder.ToString();
			var service = MainDatabaseNHibernateService.ConnectToPostgres(connectionString);

			return new MainDatabaseConnection(service);
		}


		public static async Task<MainDatabaseConnection> OpenPostgresConnectionAsync(string server, int port, string userName, string password)
		{
			return await Task.Run(() => OpenPostgresConnection(server, port, userName, password));
		}
	}
}
