using Castle.MicroKernel;
using DbAdvance.Host.Commands.Pack.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Pack.Pipeline
{
    public class PackagePipeline : BasePipeline<CommandPipelineContext>
    {
        public PackagePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<InspectParametersForPackStep>());

            RecordProcessingSteps(
                ResolveStep<CreateZipArchiveForScriptPathStep>());
        }
    }
}