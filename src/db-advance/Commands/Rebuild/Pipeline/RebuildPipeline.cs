using Castle.MicroKernel;
using DbAdvance.Host.Commands.Clean.Pipeline.Steps;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Rebuild.Pipeline.Steps;

namespace DbAdvance.Host.Commands.Rebuild.Pipeline
{
    public class RebuildPipeline
        : BasePipeline<CommandPipelineContext>
    {
        public RebuildPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("COMMAND: Rebuild Target Database");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<CleanDatabaseStep>(),
                ResolveStep<BuildInfoTablesStep>(),
                ResolveStep<InvokeUpgradeStep>());
        }
    }
}