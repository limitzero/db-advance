using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Help.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Help.Pipeline
{
    public sealed class HelpPipeline 
        : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "help", "h", "?"
        };

        public HelpPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<ShowUseageStep>());
        }
    }
}