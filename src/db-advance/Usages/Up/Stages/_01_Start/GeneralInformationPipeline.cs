using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._01_Start.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._01_Start
{
    public class GeneralInformationPipeline  : BasePipeline<CommandPipelineContext>
    {
        public GeneralInformationPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: General Information");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<DisplayInformationStep>()
                );
        }
    }
}