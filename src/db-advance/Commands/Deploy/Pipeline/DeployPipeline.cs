using Castle.MicroKernel;
using DbAdvance.Host.Commands.Deploy.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Deploy.Pipeline
{
    public class DeployPipeline : BasePipeline<CommandPipelineContext>
    {
        public DeployPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<InspectParametersForDeployStep>());

            RecordProcessingSteps(
                ResolveStep<CreateDeployDirectoryStep>(),
                ResolveStep<DeployPackageStep>());
        }
    }
}