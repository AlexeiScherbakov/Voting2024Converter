using NHibernate;

using Npgsql;

namespace RussianVotingTools.Database.FederalBlockchainVoting
{
	public class FederalBlockchainVotingConnection
		:IDisposable
	{
		private FederalBlockchainVotingNHibernateService _nHibernateService;

		private FederalBlockchainVotingConnection(FederalBlockchainVotingNHibernateService nHibernateService)
		{
			_nHibernateService = nHibernateService;
		}

		public void Dispose()
		{
			_nHibernateService.Dispose();
		}

		public ISessionFactory SessionFactory
		{
			get { return _nHibernateService.SessionFactory; }
		}


		public static async Task<FederalBlockchainVotingConnection> CreateDatabaseAsync(string connectionString)
		{
			return await Task.Run(() => Create(connectionString, true));
		}

		public static async Task<FederalBlockchainVotingConnection> OpenDatabaseAsync(string connectionString)
		{
			return await Task.Run(() => Create(connectionString, false));
		}

		public static async Task<FederalBlockchainVotingConnection> CreateDatabaseAsync(string mainConnectionString,Guid databaseUid)
		{
			return await Task.Run(() =>
			{
				NpgsqlConnectionStringBuilder builder = new(mainConnectionString);
				builder.Database = MakeDatabaseName(databaseUid);
				return Create(builder.ToString(), true);
			});
		}

		public static async Task<FederalBlockchainVotingConnection> OpenDatabaseAsync(string mainConnectionString, Guid databaseUid)
		{
			return await Task.Run(() =>
			{
				NpgsqlConnectionStringBuilder builder = new(mainConnectionString);
				builder.Database = MakeDatabaseName(databaseUid);
				return Create(builder.ToString(), false);
			});
		}

		private static FederalBlockchainVotingConnection Create(string connectionString,bool exportSchema)
		{
			var nhibernateService = FederalBlockchainVotingNHibernateService.CreatePostgres(connectionString, exportSchema);
			if (exportSchema)
			{
				using (var alterSession = nhibernateService.SessionFactory.OpenStatelessSession())
				{
					// для каждой таблицы выставляем оптимизации отключающие журналирование и откаты (т.к. данные у нас только добавляются)
					foreach (var tableName in TableName.GetAllTables())
					{
						var query = alterSession.CreateSQLQuery("ALTER TABLE " + tableName + " SET UNLOGGED;");
						query.ExecuteUpdate();
						query = alterSession.CreateSQLQuery("ALTER TABLE " + tableName + " SET (autovacuum_enabled=false);");
						query.ExecuteUpdate();
					}
				}
			}
			return new FederalBlockchainVotingConnection(nhibernateService);
		}


		public static string MakeDatabaseName(Guid uid)
		{
			return "fed_obs_db_" + uid.ToString("N");
		}
	}
}
