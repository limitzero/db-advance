using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class DatabaseConnector : IDisposable
    {
        private readonly ILogger log;
        private readonly IDatabaseConnectorConfiguration config;

        private SqlConnection connection;

        enum VersionType
        {
            CurrentVersion,
            BaseVersion
        }

        public DatabaseConnector(ILogger log, IDatabaseConnectorConfiguration config)
        {
            this.log = log;
            this.config = config;
        }

        public void Open()
        {
            connection = new SqlConnection(config.ConnectionString);
            connection.Open();
        }

        public void Apply(Step step)
        {
            log.Log("<step from='{0}' to='{1}'>", step.FromVersion, step.ToVersion);

            try
            {
                var databaseVersion = GetDatabaseVersion();

                if (step.FromVersion != databaseVersion)
                {
                    throw new InvalidOperationException(string.Format("Can not apply delta '{0}' - '{1}' to database of version '{2}'.", step.FromVersion, step.ToVersion, databaseVersion));
                }

                foreach (var script in step.Scripts)
                {
                    ExecuteScript(connection, script);
                }

                if (!DatabaseExist(config.DatabaseName) && step.ToVersion != null)
                {
                    throw new InvalidOperationException("Database {0} doesn't exist. check Please that initial delta actually creates database.");
                }

                SetVersion(VersionType.CurrentVersion, step.ToVersion);

                if (step.FromVersion == null)
                {
                    SetVersion(VersionType.BaseVersion, null);
                }
            }
            finally
            {
                log.Log("</step>", step.FromVersion, step.ToVersion);
            }
        }

        private void SetVersion(VersionType parameterName, string version)
        {
            if (!DatabaseExist(config.DatabaseName)) { return; }

            if (GetVersion(parameterName, config.DatabaseName) == null)
            {
                AddVersion(parameterName, config.DatabaseName, version);
            }
            else
            {
                UpdateVersion(parameterName, config.DatabaseName, version);
            }
        }

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

        private void AddVersion(VersionType versionType, string databaseName, string version)
        {
            UseDatabase(connection, databaseName);

            var command = new SqlCommand(@"sys.sp_addextendedproperty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

            command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
            command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

            command.ExecuteNonQuery();
        }

        private void UpdateVersion(VersionType versionType, string databaseName, string version)
        {
            UseDatabase(connection, databaseName);

            var command = new SqlCommand(@"sys.sp_updateextendedproperty", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
            command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

            command.ExecuteNonQuery();
        }

        private string GetVersion(VersionType versionType, string databaseName)
        {
            UseDatabase(connection, databaseName);

            const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = @name;";

            var command = new SqlCommand(Sql, connection)
                {
                    CommandType = CommandType.Text
                };

            command.Parameters.Add(new SqlParameter("name", GetName(versionType)));

            var version = (string)command.ExecuteScalar();

            return string.IsNullOrEmpty(version) ? null : version;
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

        private bool DatabaseExist(string databaseName)
        {
            const string Sql = @"IF (EXISTS (SELECT name 
                FROM master.dbo.sysdatabases 
                WHERE ('[' + name + ']' = @databaseName 
                OR name = @databaseName))) SELECT CAST(1 AS bit) ELSE SELECT CAST(0 AS bit);";

            var command = new SqlCommand(Sql, connection)
                {
                    CommandType = CommandType.Text
                };

            command.Parameters.Add(new SqlParameter("databaseName", databaseName));

            return (bool)command.ExecuteScalar();
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            log.Log(e.Message);
        }

        private void ExecuteScript(SqlConnection connection, ScriptAccessor scriptAccessor)
        {
            log.Log("<script name='{0}'>", scriptAccessor.ToString());

            connection.InfoMessage += InfoMessage;

            try
            {
                var script = scriptAccessor.Read();

                var commands = Regex.Split(script, @"(?m)^\s*GO\s*\d*\s*$", RegexOptions.IgnoreCase);

                foreach (var c in commands.Where(q => !string.IsNullOrEmpty(q)))
                {
                    new SqlCommand(c, connection)
                        .ExecuteNonQuery();
                }
            }
            finally
            {
                connection.InfoMessage -= InfoMessage;
                log.Log("<script/>");
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}