using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Drop.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Drop.Pipeline
{
    public class DropDatabasePipeline
        :BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "drop"
        };

        public DropDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<DropDatabaseStep>());
        }
    }
}