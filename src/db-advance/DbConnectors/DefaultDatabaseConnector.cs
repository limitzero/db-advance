using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core.Logging;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public class DefaultDatabaseConnector : BaseDatabaseConnector
    {
        public DefaultDatabaseConnector(ILogger logger, 
            IDatabaseConnectorConfiguration configuration)
            : base(logger, configuration)
        {
        }

        public override void Apply(Step step)
        {
            foreach (var script in step.Scripts)
            {
                if (!script.HasContent()) continue;

                try
                {
                    ExecuteScript(script);
                }
                catch (Exception scriptExecutionException)
                {
                    RaiseOnScriptFailure(script, scriptExecutionException);
                }
                finally
                {
                    RaiseOnScriptSuccess(script);
                }
            }
        }

        private void ExecuteScript(ScriptAccessor scriptAccessor)
        {
            using (var connection = Configuration.GetConnection())
            {                
                connection.InfoMessage += InfoMessage;

                using (var txn = connection.BeginTransaction())
                {
                    try
                    {
                        var script = scriptAccessor.Read();

                        var commands = Regex.Split(script, @"(?m)^\s*GO\s*\d*\s*$", 
                            RegexOptions.IgnoreCase);

                        foreach (var c in commands.Where(q => !string.IsNullOrEmpty(q)))
                        {
                            new SqlCommand(c, connection, txn).ExecuteNonQuery();
                        }

                        txn.Commit();
                    }
                    catch
                    {
                        txn.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.InfoMessage -= InfoMessage;
                    }
                }
            }
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Logger.Info(e.Message);
        }
    }
}