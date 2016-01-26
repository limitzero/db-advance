using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._03_PreRun.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._03_PreRun
{
    public class PreRunPipeline : BasePipeline<CommandPipelineContext>
    {
        public PreRunPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Current Database Version and Options Information");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<ReportPreUpgradeInformationStep>());
        }
    }
}