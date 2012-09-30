using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public class DefaultDatabaseConnector : BaseDatabaseConnector
    {
        public DefaultDatabaseConnector(ILogger log, IDatabaseConnectorConfiguration config)
            : base(log, config)
        {
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
            using (var connection = new SqlConnection(config.ConnectionString))
            {
                connection.Open();

                log.Log("<script name='{0}'>", scriptAccessor.ToString());

                connection.InfoMessage += InfoMessage;

                try
                {
                    var script = scriptAccessor.Read();

                    var commands = Regex.Split(script, @"(?m)^\s*GO\s*\d*\s*$", RegexOptions.IgnoreCase);

                    foreach (var c in commands.Where(q => !string.IsNullOrEmpty(q)))
                    {
                        new SqlCommand(c, connection).ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.InfoMessage -= InfoMessage;
                    log.Log("<script/>");
                }
            }
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            log.Log(e.Message);
        }
    }
}