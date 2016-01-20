using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Steps
{
    public sealed class ReportPreUpgradeInformationStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public ReportPreUpgradeInformationStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Report current information of database");

            Logger.InfoFormat("Database '{0}' on instance '{1}' is currently at version {2}.",
                _configuration.GetDatabaseName(),
                _configuration.GetDatabaseServerName(),
                GetCurrentVersionNumber());

            if (!string.IsNullOrEmpty(context.Options.Environment))
            {
                Logger.InfoFormat("'{0}' tag found for isolating environment specific scripts...",
                    context.Options.Environment);
            }

            Logger.WriteBanner();
        }

        private string GetCurrentVersionNumber()
        {
            var statement = string.Format("select top 1 v.* from [{0}] v order by id desc",
                VersionInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                var version = connection.Query<VersionInfo>(statement)
                    .FirstOrDefault();

                if (version == null)
                    return "0";
                else
                    return version.Version;
            }
        }
    }
}