using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rollback.Pipeline.Steps
{
    public class RemoveRolledBackVersionInformationStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public RemoveRolledBackVersionInformationStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
        }

        private IEnumerable<VersionInfo> GetVersionsToRemoveOnRollback(CommandPipelineContext context)
        {
            var startVersionStatement = string.Format("select top 1v.* from [{1}] v where [Version] = '{0}'",
                context.FromVersion,
                VersionInfo.GetTableName());

            var endVersionStatement = string.Format("select top 1v.* from [{1}] v where [Version] = '{0}'",
                context.ToVersion,
                VersionInfo.GetTableName());
            
            return null;
        }

        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}