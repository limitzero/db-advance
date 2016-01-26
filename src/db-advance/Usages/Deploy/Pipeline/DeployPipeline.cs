using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Deploy.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Deploy.Pipeline
{
    public class DeployPipeline : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "deploy"
        };

        public DeployPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Deploy");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<InspectParametersForDeployStep>());

            RecordProcessingSteps(
                ResolveStep<PrepareDeploymentStep>(),
                ResolveStep<DeployPackageStep>());
        }
    }
}