using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rollback.Pipeline.Steps
{
    public class GetVersionsWithTagStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public GetVersionsWithTagStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration)
            : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Determine migration version information for rollback");

            DetermineVersionRollbackRange(context);

            Logger.WriteBanner();
        }

        private void DetermineVersionRollbackRange(CommandPipelineContext context)
        {
            var databaseVersion = GetDatabaseVersion();
            var versions = GetVersionsToRollback(context);

            if (context.Options.VersionsToRollback == 0)
            {
                Logger.Warn("Specified number of versions to rollback not stated. Aborting...");
                HaltPipeline = true;
            }
            else if (!versions.Any())
            {
                Logger.Warn("No version information recorded for rollback. Aborting...");
                HaltPipeline = true;
            }

            if (databaseVersion != null & versions.Any())
            {
                context.FromVersion = databaseVersion.Version;
                context.ToVersion = versions.Last().Version;
                Logger.InfoFormat("Rolling back target database from '{0}' to '{1}'.",
                    context.FromVersion,
                    context.ToVersion);
            }

            if (context.Options.Tags.Any())
            {
                Logger.InfoFormat("Tag(s) used on rollback: {0}", string.Join(",", context.Options.Tags));
            }
        }

        private IEnumerable<VersionInfo> GetVersionsToRollback(CommandPipelineContext context)
        {
            var statement = string.Format("select top {0} v.* from [{1}] v order by version desc",
                context.Options.VersionsToRollback,
                VersionInfo.GetTableName());

            using (var connection = GetConnection())
            {
                return connection.Query<VersionInfo>(statement).ToList();
            }
        }

        private VersionInfo GetDatabaseVersion()
        {
            var statement = string.Format("select top 1 v.* from [{0}] v order by id desc",
                VersionInfo.GetTableName());

            using (var connection = GetConnection())
            {
                var max = connection.Query<VersionInfo>(statement).FirstOrDefault();
                return max;
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