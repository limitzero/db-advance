using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Create.Pipeline;
using DbAdvance.Host.Usages.Drop.Pipeline;
using DbAdvance.Host.Usages.Up.Stages;

namespace DbAdvance.Host.Usages.Restore.Pipeline
{
    public class RestoreDatabasePipeline : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "rebuild", "rb"
        };

        public RestoreDatabasePipeline(IKernel kernel) : base(kernel)
        {
           
        }

        public override void Configure()
        {
            RecordPipelineChannel<DropDatabasePipeline>();
            RecordPipelineChannel<CreateDatabasePipeline>();
            // setup the schema tables pipeline....
            RecordPipelineChannel<UpPipeline>();
        }
    }
}