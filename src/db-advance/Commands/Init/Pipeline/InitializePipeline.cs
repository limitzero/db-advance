using Castle.MicroKernel;
using DbAdvance.Host.Commands.Init.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Init.Pipeline
{
    public class InitializePipeline : BasePipeline<CommandPipelineContext>
    {
        public InitializePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<ConstructScriptFoldersOnPathStep>());
        }
    }
}