using System.Data.SqlClient;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Setup.Pipeline.Steps
{
    public sealed class InspectSchemaStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public InspectSchemaStep(IKernel kernel, IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            context.IsSchemaPresent =
                IsVersionInfoTablePresent() &&
                IsScriptRunInfoTablePresent() &&
                IsScriptRunErrorInfoTablePresent() &&
                IsScriptRunDeployInfoTablePresent();
        }

        private bool IsVersionInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                VersionInfo.GetTableName());

            using (var connection = GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunInfo.GetTableName());

            using (var connection = GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunErrorInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunErrorInfo.GetTableName());

            using (var connection = GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private bool IsScriptRunDeployInfoTablePresent()
        {
            var statement = string.Format(
                @"SELECT Present = Count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')",
                ScriptsRunDeployInfo.GetTableName());

            using (var connection = GetConnection())
            {
                return connection.Query<int>(statement).FirstOrDefault() > 0;
            }
        }

        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}