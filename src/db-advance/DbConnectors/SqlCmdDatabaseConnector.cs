using System;
using System.Data.SqlClient;
using Castle.Core.Logging;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public class SqlCmdDatabaseConnector : BaseDatabaseConnector
    {
        private readonly SqlCmdRunner _runner;
        private readonly SqlConnectionStringBuilder _сonnectionStringBuilder;

        public SqlCmdDatabaseConnector(SqlCmdRunner runner,
            ILogger logger,
            IDatabaseConnectorConfiguration configuration)
            : base(logger, configuration)
        {
            _runner = runner;
            _сonnectionStringBuilder = new SqlConnectionStringBuilder(configuration.ConnectionString);
        }

        public override void Apply(Step step)
        {
            var databaseExists = DatabaseExists();

            foreach (var script in step.Scripts)
            {
                if (!script.HasContent()) continue;

                try
                {
                    if (databaseExists)
                    {
                        var executeScriptResult = ExecuteScript(script);
                        ExamineForScriptError(executeScriptResult);
                    }
                    else
                    {
                        var executeScriptOnMasterResult = ExecuteScriptOnMaster(script);
                        ExamineForScriptError(executeScriptOnMasterResult);
                    }
                }
                catch (Exception scriptException)
                {
                    RaiseOnScriptFailure(script, scriptException);
                }

                RaiseOnScriptSuccess(script);
            }
        }

        private void ExamineForScriptError(string result)
        {
            if (result.StartsWith("Msg")) // SQL Engine for SqlCmd standard starting text
                throw new InvalidOperationException(result);
        }

        private string ExecuteScriptOnMaster(ScriptAccessor scriptAccessor)
        {
            var output = _runner.Run(
                _сonnectionStringBuilder.DataSource,
                _сonnectionStringBuilder.UserID,
                _сonnectionStringBuilder.Password,
                scriptAccessor.GetFullPath());

            return output;
        }

        private string ExecuteScript(ScriptAccessor scriptAccessor)
        {
            var output = _runner.Run(
                _сonnectionStringBuilder.DataSource,
                _сonnectionStringBuilder.UserID,
                _сonnectionStringBuilder.Password,
                scriptAccessor.GetFullPath(),
                GetDatabaseName());

            return output;
        }
    }
}