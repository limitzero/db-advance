using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Create.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Create.Pipeline
{
    public sealed class CreateDatabasePipeline 
        : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "create"
        };

        public CreateDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<CreateDatabaseStep>());
        }
    }
}