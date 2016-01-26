using System.IO;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._02_Setup.Steps
{
    public class CreateInfoTablesStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DatabaseConnectorFactory _factory;

        public CreateInfoTablesStep(IKernel kernel, 
            IDatabaseConnectorConfiguration configuration, 
            DatabaseConnectorFactory factory) : base(kernel)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
            CheckForVersionInfoTable();
            CheckForScriptsRunDeployInfoTable();
            CheckForScriptsRunInfoTable();
            CheckForScriptsRunErrorInfoTable();
        }

        private void CheckForVersionInfoTable()
        {
            Logger.InfoFormat("Creating [{0}] table, if it does not exist..", VersionInfo.GetTableName());
            CreateInfoTable("versioninfo_create.sql");
        }

        private void CheckForScriptsRunInfoTable()
        {
            Logger.InfoFormat("Creating [{0}] table, if it does not exist..", ScriptsRunInfo.GetTableName());
            CreateInfoTable("scriptsruninfo_create.sql");
        }

        private void CheckForScriptsRunErrorInfoTable()
        {
            Logger.InfoFormat("Creating [{0}] table, if it does not exist..", ScriptsRunErrorInfo.GetTableName());
            CreateInfoTable("scriptsrunerrorinfo_create.sql");
        }

        private void CheckForScriptsRunDeployInfoTable()
        {
            Logger.InfoFormat("Creating [{0}] table, if it does not exist..", ScriptsRunDeployInfo.GetTableName());
            CreateInfoTable("scriptsrundeployinfo_create.sql");
        }

        private bool IsVersionInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                VersionInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunErrorInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunErrorInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunDeployInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunDeployInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private void CreateInfoTable(string scriptName)
        {
            var script = GetSchemaScriptContent(scriptName);
            var connector = _factory.UseBasicConnector();
            connector.Apply(script);
        }

        private string GetSchemaScriptContent(string scriptName)
        {
            var resource = GetType().Assembly.GetManifestResourceNames()
              .FirstOrDefault(rn => rn.EndsWith(scriptName));

            using (var stream = GetType().Assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}