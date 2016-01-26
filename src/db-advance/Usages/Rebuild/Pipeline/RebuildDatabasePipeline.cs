using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Rebuild.Pipeline.Steps;
using DbAdvance.Host.Usages.Up.Stages;

namespace DbAdvance.Host.Usages.Rebuild.Pipeline
{
    public class RebuildDatabasePipeline : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "rebuild", "rb"
        };

        public RebuildDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("---: Rebuilding Target Database :---");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<DropAndCreateDatabaseStep>(), 
                ResolveStep<InvokeUpgradeStep>());
        }
    }
}