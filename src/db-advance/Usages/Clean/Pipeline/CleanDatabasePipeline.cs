using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Create.Pipeline;
using DbAdvance.Host.Usages.Drop.Pipeline;

namespace DbAdvance.Host.Usages.Clean.Pipeline
{
    public sealed class CleanDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "clean"
        };

        public CleanDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE - CLEAN:  Drop & Re-create Target Database");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordPipelineChannel<DropDatabasePipeline>();
            RecordPipelineChannel<CreateDatabasePipeline>();
        }
    }
}