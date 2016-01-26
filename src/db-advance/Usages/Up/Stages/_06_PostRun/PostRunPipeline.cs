using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._06_PostRun.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._06_PostRun
{
    public class PostRunPipeline : BasePipeline<CommandPipelineContext>
    {
        public PostRunPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<ReportPostUpgradeInformationStep>());
        }
    }
}