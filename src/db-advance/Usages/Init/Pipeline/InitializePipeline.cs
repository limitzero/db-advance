using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Init.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Init.Pipeline
{
    public class InitializePipeline : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "i", "init"
        };

        public InitializePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<ConstructScriptFoldersOnPathStep>());
        }
    }
}