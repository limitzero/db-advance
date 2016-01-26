using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._05_Version.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._05_Version
{
    public class VersionPipeline : BasePipeline<CommandPipelineContext>
    {
        public VersionPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Versioning");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<VersionAllScriptsForRunStep>());
        }
    }
}