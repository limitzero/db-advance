using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Backup.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Backup.Pipeline
{
    public class BackupDatabasePipeline
         : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "backup"
        };

        public BackupDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Backup Target Database");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<BackupDatabaseStep>());
        }
    }
}