using Castle.MicroKernel;
using DbAdvance.Host.Commands.Clean.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Clean.Pipeline
{
    public sealed class CleanDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        public CleanDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<CleanDatabaseStep>());
        }
    }
}