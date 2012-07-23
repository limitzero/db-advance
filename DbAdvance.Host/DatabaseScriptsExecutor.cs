using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbAdvance.Host
{
    public class DatabaseScriptsExecutor
    {
        private readonly ILogger log;

        public DatabaseScriptsExecutor(ILogger log)
        {
            this.log = log;
        }

        public void CleanAllDatabasesFromServer(string connectionString)
        {
            log.Log("Removing all databases from server...");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                const string Sql = @"
                    EXEC sp_MSforeachdb '
                    IF (DB_ID(''?'') > 4)
                    BEGIN
                       print (''?'')
                       print (DB_ID(''?''))
                       EXEC (''ALTER DATABASE ? SET SINGLE_USER WITH ROLLBACK IMMEDIATE'')
                       EXEC (''DROP DATABASE ?'')
                    END'
                    ";

                new SqlCommand(Sql, connection)
                    .ExecuteNonQuery();
            }

            log.Log("Done.");
        }

        public void ApplyDelta(string databaseName, string connectionString, string fromVersion, string toVersion, IEnumerable<string> deltaContents)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.InfoMessage += InfoMessage;

                string databaseVersion = null;

                if (DoesDatabaseExist(connection, databaseName))
                {
                    databaseVersion = GetVersion(connection, databaseName);
                }

                log.Log("DB: Database is of '{0}' version. ", databaseVersion);

                if (fromVersion != databaseVersion)
                {
                    throw new InvalidOperationException(string.Format("Can not apply delta '{0}' - '{1}' to database of version '{2}'.", fromVersion, toVersion, databaseVersion));
                }

                log.Log("DB: Applying '{0}' version. ", toVersion);

                foreach (var script in deltaContents)
                {
                    ExecuteScript(connection, script);
                }

                if (DoesDatabaseExist(connection, databaseName))
                {
                    if (fromVersion == null)
                    {
                        AddVersion(connection, toVersion, databaseName);
                    }
                    else
                    {
                        UpdateVersion(connection, toVersion, databaseName);
                    }
                }

                if (!DoesDatabaseExist(connection, databaseName) && toVersion != null)
                {
                    throw new InvalidOperationException("Database {0} doesn't exist. check Please that initial delta actually creates database.");
                }

                log.Log("DB: '{0}' version was applied. ", toVersion);
            }
        }

        private static void AddVersion(SqlConnection connection, string version, string databaseName)
        {
            UseDatabase(connection, databaseName);

            var command = new SqlCommand(@"sys.sp_addextendedproperty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

            command.Parameters.Add(new SqlParameter("name", "DB_VERSION"));
            command.Parameters.Add(new SqlParameter("value", version));

            command.ExecuteNonQuery();
        }

        private static void UpdateVersion(SqlConnection connection, string version, string databaseName)
        {
            UseDatabase(connection, databaseName);

            var command = new SqlCommand(@"sys.sp_updateextendedproperty", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("name", "DB_VERSION"));
            command.Parameters.Add(new SqlParameter("value", version));

            command.ExecuteNonQuery();
        }

        private static string GetVersion(SqlConnection connection, string databaseName)
        {
            UseDatabase(connection, databaseName);
            
            const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = N'DB_VERSION';";

            var command = new SqlCommand(Sql, connection)
                {
                    CommandType = CommandType.Text
                };

            command.Parameters.Add(new SqlParameter("name", "DB_VERSION"));

            var version = (string)command.ExecuteScalar();
            
            return string.IsNullOrEmpty(version) ? null : version;
        }

        private static void UseDatabase(SqlConnection connection, string databaseName)
        {
            var sql = string.Format("use {0};", databaseName);

            var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.ExecuteNonQuery();
        }

        private static bool DoesDatabaseExist(SqlConnection connection, string databaseName)
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

        private static string ReadScript(string scriptPath)
        {
            using (var reader = new StreamReader(scriptPath))
            {
                return reader.ReadToEnd();
            }
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            log.Log(e.Message);
        }

        private void ExecuteScript(SqlConnection connection, string scriptPath)
        {
            log.Log("DB: Executing '{0}'... ", scriptPath);

            var script = ReadScript(scriptPath);

            var commands = Regex.Split(script, @"(?m)^\s*GO\s*\d*\s*$", RegexOptions.IgnoreCase);

            foreach (var c in commands.Where(q => !string.IsNullOrEmpty(q)))
            {
                new SqlCommand(c, connection)
                    .ExecuteNonQuery();
            }

            log.Log("Script Applied");
        }
    }
}