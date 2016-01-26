using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._06_PostRun.Steps
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
        }
    }
}