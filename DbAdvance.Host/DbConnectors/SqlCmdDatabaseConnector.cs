using System;
using System.Data.SqlClient;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public class SqlCmdDatabaseConnector : BaseDatabaseConnector
    {
        private readonly SqlCmdRunner runner;
        private readonly SqlConnectionStringBuilder сonnectionStringBuilder;

        public SqlCmdDatabaseConnector(SqlCmdRunner runner, ILogger log, IDatabaseConnectorConfiguration config)
            : base(log, config)
        {
            this.runner = runner;
            сonnectionStringBuilder = new SqlConnectionStringBuilder(config.ConnectionString);
        }

        public override void Apply(Step step)
        {
            log.Log("<step from='{0}' to='{1}'>", step.FromVersion, step.ToVersion);

            try
            {
                var databaseVersion = GetDatabaseVersion();

                if (step.FromVersion != databaseVersion)
                {
                    throw new InvalidOperationException(string.Format("Cannot apply delta '{0}' - '{1}' to database of version '{2}'.", step.FromVersion, step.ToVersion, databaseVersion));
                }

                foreach (var script in step.Scripts)
                {
                    ExecuteScript(script);
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

        private void ExecuteScript(ScriptAccessor scriptAccessor)
        {
            log.Log("<script name='{0}'>", scriptAccessor.ToString());

            try
            {
                runner.Run(
                    сonnectionStringBuilder.DataSource,
                    сonnectionStringBuilder.UserID,
                    сonnectionStringBuilder.Password,
                    scriptAccessor.GetFullPath());
            }
            finally
            {
                log.Log("<script/>");
            }
        }
    }
}