using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Backup.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._01_Start;
using DbAdvance.Host.Usages.Up.Stages._03_PreRun;
using DbAdvance.Host.Usages.Up.Stages._04_Migrate;
using DbAdvance.Host.Usages.Up.Stages._05_Version;
using DbAdvance.Host.Usages.Up.Stages._06_PostRun;
using DbAdvance.Host.Usages._Setup.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages
{
    public class UpPipeline :
         BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "up"
        };

        public UpPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordPipelineChannel<GeneralInformationPipeline>();
            RecordPipelineChannel<SetupDatabasePipeline>();
            RecordPipelineChannel<BackupDatabasePipeline>();
            RecordPipelineChannel<PreRunPipeline>();
            RecordPipelineChannel<MigratePipeline>();
            RecordPipelineChannel<VersionPipeline>();
            RecordPipelineChannel<PostRunPipeline>();
        }
    }
}