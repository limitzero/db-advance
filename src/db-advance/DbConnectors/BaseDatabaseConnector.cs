using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Castle.Core.Logging;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public abstract class BaseDatabaseConnector : IDatabaseConnector
    {
        public event Action<ScriptRunResult> OnScriptExecuted;

        protected ILogger Logger { get; private set; }
        protected IDatabaseConnectorConfiguration Configuration { get; private set; }

        protected BaseDatabaseConnector(ILogger logger, IDatabaseConnectorConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        public void Apply(string statement)
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(file, statement);

            var scriptAccessor = new ScriptAccessor(file);

            try
            {
                this.Apply(new Step {Scripts = new[] {scriptAccessor}});
                RaiseOnScriptSuccess(scriptAccessor);
            }
            catch (Exception scriptApplicationException)
            {
                RaiseOnScriptFailure(scriptAccessor, scriptApplicationException);
            }
            finally
            {
                File.Delete(file);
            }
        }

        protected void RaiseOnScriptSuccess(ScriptAccessor script)
        {
            var evt = this.OnScriptExecuted;
            if (evt != null)
                evt(new ScriptRunResult {Script = script});
        }

        protected void RaiseOnScriptFailure(ScriptAccessor script, Exception exception)
        {
            var evt = OnScriptExecuted;
            if (evt != null)
            {
                var result = new ScriptRunResult {Script = script};
                result.RecordError(exception.Message);
                evt(result);
            }
        }

        public abstract void Apply(Step step);

        public string GetDatabaseVersion()
        {
            string databaseVersion = null;

            if (DatabaseExists())
            {
                databaseVersion = GetVersion(VersionType.CurrentVersion, GetDatabaseName());
            }

            return databaseVersion;
        }

        public string GetBaseDatabaseVersion()
        {
            string databaseVersion = null;

            if (DatabaseExists())
            {
                databaseVersion = GetVersion(VersionType.BaseVersion, GetDatabaseName());
            }

            return databaseVersion;
        }

        public void SetBaseDatabaseVersion(string version)
        {
            SetVersion(VersionType.BaseVersion, version);
        }

        protected void SetVersion(VersionType parameterName, string version)
        {
            if (!DatabaseExists())
            {
                return;
            }

            if (VersionMissing(parameterName, Configuration.DatabaseName))
            {
                AddVersion(parameterName, Configuration.DatabaseName, version);
            }
            else
            {
                UpdateVersion(parameterName, Configuration.DatabaseName, version);
            }
        }

        public bool DatabaseExists()
        {
            using (var currentConnection = new SqlConnection(Configuration.ConnectionString))
            {
                currentConnection.Open();
                var database = currentConnection.Database;

                const string Sql = @"IF (EXISTS (SELECT name 
                FROM master.dbo.sysdatabases 
                WHERE ('[' + name + ']' = @databaseName 
                OR name = @databaseName))) SELECT CAST(1 AS bit) ELSE SELECT CAST(0 AS bit);";

                var command = new SqlCommand(Sql, currentConnection) {CommandType = CommandType.Text};

                command.Parameters.Add(new SqlParameter("databaseName", database));

                return (bool) command.ExecuteScalar();
            }
        }

        public string GetDatabaseName()
        {
            var connection = new SqlConnectionStringBuilder(Configuration.ConnectionString);
            return connection.InitialCatalog;
        }

        private void AddVersion(VersionType versionType, string databaseName, string version)
        {
            using (var currentConnection = new SqlConnection(Configuration.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                var command = new SqlCommand(@"sys.sp_addextendedproperty", currentConnection)
                {CommandType = CommandType.StoredProcedure};

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
                command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

                command.ExecuteNonQuery();
            }
        }

        private void UpdateVersion(VersionType versionType, string databaseName, string version)
        {
            using (var currentConnection = new SqlConnection(Configuration.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                var command = new SqlCommand(@"sys.sp_updateextendedproperty", currentConnection)
                {CommandType = CommandType.StoredProcedure};

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));
                command.Parameters.Add(new SqlParameter("value", version ?? string.Empty));

                command.ExecuteNonQuery();
            }
        }

        private bool VersionMissing(VersionType versionType, string databaseName)
        {
            using (var currentConnection = new SqlConnection(Configuration.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = @name;";

                var command = new SqlCommand(Sql, currentConnection) {CommandType = CommandType.Text};

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));

                var version = (string) command.ExecuteScalar();

                return version == null;
            }
        }

        private string GetVersion(VersionType versionType, string databaseName)
        {
            using (var currentConnection = new SqlConnection(Configuration.ConnectionString))
            {
                currentConnection.Open();

                UseDatabase(currentConnection, databaseName);

                const string Sql = @"SELECT CAST(Value AS nvarchar(500))
                FROM sys.extended_properties AS ep
                WHERE ep.name = @name;";

                var command = new SqlCommand(Sql, currentConnection) {CommandType = CommandType.Text};

                command.Parameters.Add(new SqlParameter("name", GetName(versionType)));

                var version = (string) command.ExecuteScalar();

                return string.IsNullOrEmpty(version) ? null : version;
            }
        }

        private static string GetName(VersionType versionType)
        {
            return Enum.GetName(typeof (VersionType), versionType);
        }

        private static void UseDatabase(SqlConnection connection, string databaseName)
        {
            var sql = string.Format("use {0};", databaseName);

            var command = new SqlCommand(sql, connection) {CommandType = CommandType.Text};

            command.ExecuteNonQuery();
        }

        protected enum VersionType
        {
            CurrentVersion,
            BaseVersion
        }
    }
}