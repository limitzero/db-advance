using Castle.MicroKernel;
using DbAdvance.Host.Commands.Rebuild.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rebuild.Pipeline
{
    public class RebuildPipeline
        : BasePipeline<CommandPipelineContext>
    {
        public RebuildPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<RecreateDatabaseStep>(),
                ResolveStep<BuildInfoTablesStep>(),
                ResolveStep<InvokeUpgradeStep>());
        }
    }
}