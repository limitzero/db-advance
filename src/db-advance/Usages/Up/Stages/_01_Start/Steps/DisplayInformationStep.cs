using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._01_Start.Steps
{
    public class DisplayInformationStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public DisplayInformationStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.InfoFormat(
                "Executing db-advance v{0} against instance {1} for database {2}...",
                GetType().Assembly.GetName().Version.ToString(),
                _configuration.GetDatabaseServerName(),
                _configuration.GetDatabaseName());

            Logger.InfoFormat(
                "Looking in current path '{0}' for all scripts to run...",
                context.Options.ScriptsPath);
        }
    }
}