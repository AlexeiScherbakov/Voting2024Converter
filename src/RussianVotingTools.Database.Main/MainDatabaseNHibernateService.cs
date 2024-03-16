using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class MainDatabaseNHibernateService
		:IDisposable
	{
		private readonly static HbmMapping _mapping;


		static MainDatabaseNHibernateService()
		{
			_mapping = CreateMapping();
		}

		private static HbmMapping CreateMapping()
		{
			var mapper = new ModelMapper();

			mapper.Class<DbElectionTimeline>(clazz =>
			{
				clazz.Table(TableName.ElectionTimeline);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Identity);
				});
				clazz.Property(x => x.Name, map =>
				{
					map.Column(ColumnName.Name);
					map.NotNullable(true);
				});
				clazz.Property(x => x.PlannedStartTime, map =>
				{
					map.Column("PlannedStartTime");
					map.NotNullable(true);
				});
				clazz.Property(x => x.PlannedEndTime, map =>
				{
					map.Column("PlannedEndTime");
					map.NotNullable(true);
				});
				clazz.Property(x => x.StartTime, map =>
				{
					map.Column("StartTime");
					map.NotNullable(false);
				});
				clazz.Property(x => x.EndTime, map =>
				{
					map.Column("EndTime");
					map.NotNullable(false);
				});
				clazz.Set(x => x.ElectionObservations, map =>
				{
					map.Table(TableName.ElectionObservation);
					map.Key(xx => xx.Column(ColumnName.ElectionTimelineId));
					map.Inverse(true);
				}, relation => relation.OneToMany());
			});

			mapper.Class<DbElectionObservation>(clazz =>
			{
				clazz.Table(TableName.ElectionObservation);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Identity);
				});
				clazz.Property(x => x.Name, map =>
				{
					map.Column(ColumnName.Name);
					map.NotNullable(true);
				});
				clazz.ManyToOne(x => x.ElectionTimeline, map =>
				{
					map.Column(ColumnName.ElectionTimelineId);
					map.Lazy(LazyRelation.Proxy);
					map.Index($"idx_{TableName.ElectionObservation}_{ColumnName.ElectionTimelineId}");
				});
			});

			mapper.Class<DbBlockchainObservation>(clazz =>
			{
				clazz.Table(TableName.BlockchainObservation);
				clazz.Id(x => x.ID, map =>
				{
					map.Column(ColumnName.ID);
					map.Generator(Generators.Identity);
				});
				clazz.Property(x => x.Name, map =>
				{
					map.Column(ColumnName.Name);
					map.NotNullable(true);
				});
				clazz.Property(x => x.Uid, map =>
				{
					map.Column(ColumnName.Uid);
					map.NotNullable(true);
				});
				clazz.ManyToOne(x => x.ElectionObservation, map =>
				{
					map.Column(ColumnName.ElectionObservationId);
					map.Lazy(LazyRelation.Proxy);
					map.Index($"idx_{TableName.BlockchainObservation}_{ColumnName.ElectionObservationId}");
				});
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}


		private string _connectionString;
		private readonly ISessionFactory _sessionFactory;

		private MainDatabaseNHibernateService(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory;
		}

		public void Dispose()
		{
			_sessionFactory.Dispose();
		}

		public ISessionFactory SessionFactory
		{
			get { return _sessionFactory; }
		}

		public string ConnectionString
		{
			get { return _connectionString; }
		}

		public async Task<bool> IsDatabaseExistsAsync(string databaseName)
		{
			//SELECT datname FROM pg_catalog.pg_database WHERE datname='dbname'
			using var session = _sessionFactory.OpenSession();
			using var cmd = session.Connection.CreateCommand();
			cmd.CommandText = "SELECT datname FROM pg_catalog.pg_database WHERE datname=@dbname";
			var parameter = cmd.CreateParameter();
			parameter.ParameterName = "@dbname";
			parameter.Value = databaseName;
			cmd.Parameters.Add(parameter);
			var datname = (string?) await cmd.ExecuteScalarAsync();
			return datname is not null;
		}


		public static MainDatabaseNHibernateService ConnectToPostgres(string connectionString)
		{
			Configuration cfg = new();
			cfg.DataBaseIntegration(
				db =>
				{
					db.MaximumDepthOfOuterJoinFetching = 6;
					db.ConnectionString = connectionString;
					db.ConnectionProvider<DriverConnectionProvider>();
					db.Driver<NHibernate.Driver.NpgsqlDriver>();
					db.Dialect<NHibernate.Dialect.PostgreSQL83Dialect>();
				})
				.Proxy(x => x.ProxyFactoryFactory<NHibernate.Bytecode.StaticProxyFactoryFactory>());
			cfg.AddMapping(_mapping);

			var sessionFactory = cfg.BuildSessionFactory();
			var ret = new MainDatabaseNHibernateService(sessionFactory);
			ret._connectionString = connectionString;
			return ret;
		}
	}
}
