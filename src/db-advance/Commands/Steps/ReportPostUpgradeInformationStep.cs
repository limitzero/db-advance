using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Steps
{
    public sealed class ReportPostUpgradeInformationStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public ReportPostUpgradeInformationStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Report migration status");

            if (context.AllScriptsRun.Any())
            {
                Logger.InfoFormat("Database '{0}' on instance '{1}' has been updated to version '{2}'.",
                    _configuration.GetDatabaseName(),
                    _configuration.GetDatabaseServerName(),
                    context.ToVersion);
            }
            else
            {
                Logger.InfoFormat("No new or changed scripts found to apply for database '{0}' on  instance '{1}'...",
                    _configuration.GetDatabaseName(),
                    _configuration.GetDatabaseServerName());
            }

            Logger.WriteBanner();
        }
    }
}