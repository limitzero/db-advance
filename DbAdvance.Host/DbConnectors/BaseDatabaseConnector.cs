using System;
using System.Data;
using System.Data.SqlClient;

namespace DbAdvance.Host.DbConnectors
{
    public abstract class BaseDatabaseConnector : IDatabaseConnector
    {
        protected readonly ILogger log;
        protected readonly IDatabaseConnectorConfiguration config;

        protected BaseDatabaseConnector(ILogger log, IDatabaseConnectorConfiguration config)
        {
            this.log = log;
            this.config = config;
        }

        public abstract void Apply(Step step);

        public string GetDatabaseVersion()
        {
            string databaseVersion = null;

            if (DatabaseExist(config.DatabaseName))
            {
                databaseVersion = GetVersion(VersionType.CurrentVersion, config.DatabaseName);
            }

            return databaseVersion;
        }

        public string GetBaseDatabaseVersion()
        {
            string databaseVersion = null;

            if (DatabaseExist(config.DatabaseName))
            {
                databaseVersion = GetVersion(VersionType.BaseVersion, config.DatabaseName);
            }

            return databaseVersion;
        }

        public void SetBaseDatabaseVersion(string version)
        {
            SetVersion(VersionType.BaseVersion, version);
        }
        
        protected void SetVersion(VersionType parameterName, string version)
        {
            if (!DatabaseExist(config.DatabaseName)) { return; }

            if (VersionMissing(parameterName, config.DatabaseName))
            {
                AddVersion(parameterName, config.DatabaseName, version);
            }
            else
            {
                UpdateVersion(parameterName, config.DatabaseName, version);
            }
        }

        protected bool DatabaseExist(string databaseName)
        {
            using (var currentConnection = new SqlConnection(config.ConnectionString))
            {
                currentConnection.Open();

                const string Sql = @"IF (EXISTS (SELECT name 
                FROM master.dbo.sysdatabases 
                WHERE ('[' + name + ']' = @databaseName 
                OR name = @databaseName))) SELECT CAST(1 AS bit) ELSE SELECT CAST(0 AS bit);";

                var command = new SqlCommand(Sql, currentConnection) { CommandType = CommandType.Text };

                command.Parameters.Add(new SqlParameter("databaseName", databaseName));

                return (bool)command.ExecuteScalar();
            }
        }

        private void AddVersion(VersionType versionType, string databaseName, string version)
        {
            using (var currentConnection = new SqlConnection(config.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                var command = new SqlCommand(@"sys.sp_addextendedproperty", currentConnection)
                    { CommandType = CommandType.StoredProcedure };

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
                command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

                command.ExecuteNonQuery();
            }
        }

        private void UpdateVersion(VersionType versionType, string databaseName, string version)
        {
            using (var currentConnection = new SqlConnection(config.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                var command = new SqlCommand(@"sys.sp_updateextendedproperty", currentConnection)
                    { CommandType = CommandType.StoredProcedure };

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
                command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

                command.ExecuteNonQuery();
            }
        }

        private bool VersionMissing(VersionType versionType, string databaseName)
        {
            using (var currentConnection = new SqlConnection(config.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = @name;";

                var command = new SqlCommand(Sql, currentConnection) { CommandType = CommandType.Text };

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));

                var version = (string)command.ExecuteScalar();

                return version == null;
            }
        }

        private string GetVersion(VersionType versionType, string databaseName)
        {
            using (var currentConnection = new SqlConnection(config.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = @name;";

                var command = new SqlCommand(Sql, currentConnection) { CommandType = CommandType.Text };

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));

                var version = (string)command.ExecuteScalar();

                return string.IsNullOrEmpty(version) ? null : version;
            }
        }

        private static string GetName(VersionType versionType)
        {
            return Enum.GetName(typeof(VersionType), versionType);
        }

        private static void UseDatabase(SqlConnection connection, string databaseName)
        {
            var sql = string.Format("use {0};", databaseName);

            var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.ExecuteNonQuery();
        }

        protected enum VersionType
        {
            CurrentVersion,
            BaseVersion
        }
    }
}