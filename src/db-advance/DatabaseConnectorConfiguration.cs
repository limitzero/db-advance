using System;
using DbAdvance.Host.DbConnectors;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using DbAdvance.Host.Usages;
using DbAdvance.Host.Usages.Help.Pipeline;

namespace DbAdvance.Host
{
    public class DatabaseConnectorConfiguration : IDatabaseConnectorConfiguration
    {
        private readonly DbAdvancedOptions _options;
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string DbConnectorType
        {
            get { return ConfigurationManager.AppSettings["DbConnectorType"]; }
        }

        public DatabaseConnectorConfiguration(DbAdvancedOptions options)
        {
            _options = options;
            Configure(_options);
        }

        public void Configure(DbAdvancedOptions options)
        {
            if(!HelpPipeline.CommandAliases.Contains(options.Command))
            GetConnectionFromDatabaseName(options);
        }

        private void GetConnectionFromDatabaseName(DbAdvancedOptions options)
        { 
            if(string.IsNullOrEmpty(options.ConnectionString) &
                string.IsNullOrEmpty(options.Database))
                throw new ArgumentException(
                    "The command line arguement for specifying the target database connection was not supplied");

            if (string.IsNullOrEmpty(options.Database) & !string.IsNullOrEmpty(options.ConnectionString))
            {
                ConnectionString = options.ConnectionString;
                return;
            }

            if (!string.IsNullOrEmpty(options.Database) & string.IsNullOrEmpty(options.ConnectionString))
            {
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