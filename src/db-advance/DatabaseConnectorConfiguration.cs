using System;
using DbAdvance.Host.DbConnectors;
using System.Configuration;
using System.Data.SqlClient;

namespace DbAdvance.Host
{
    public class DatabaseConnectorConfiguration : IDatabaseConnectorConfiguration
    {
        private readonly DbAdvanceCommandLineOptions _options;
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string DbConnectorType
        {
            get { return ConfigurationManager.AppSettings["DbConnectorType"]; }
        }

        public DatabaseConnectorConfiguration(DbAdvanceCommandLineOptions options)
        {
            _options = options;
            Configure(_options);
        }

        public void Configure(DbAdvanceCommandLineOptions options)
        {
            GetConnectionFromDatabaseName(options);
        }

        private void GetConnectionFromDatabaseName(DbAdvanceCommandLineOptions options)
        {
            var connection = string.Empty;

            if (string.IsNullOrEmpty(options.Database))
                throw new ArgumentException(
                    "The command line arguement for specifying the target database was not supplied.");

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[options.Database].ConnectionString;
            }
            catch
            {
                throw new ArgumentException(
                    string.Format(
                        "The connection setting for database via name '{0}' was not specified in the configuration settings file. ",
                        options.Database));
            }

            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentException(
                    string.Format(
                        "The database connection string was not supplied for connection setting via name '{0}' in the configuration settings file. ",
                        options.Database));
        }

        public string GetDatabaseName()
        {
            var connection = new SqlConnectionStringBuilder(ConnectionString);
            return connection.InitialCatalog;
        }

        public string GetDatabaseServerName()
        {
            var connection = new SqlConnectionStringBuilder(ConnectionString);
            return connection.DataSource; 
        }

        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public SqlConnection GetConnectionToMaster()
        {
            var masterConnectionString = new SqlConnectionStringBuilder(ConnectionString);
            masterConnectionString.InitialCatalog = "master";

            var connection = new SqlConnection(masterConnectionString.ToString());
            connection.Open();
            return connection;
        }
    }
}