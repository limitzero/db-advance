using Castle.MicroKernel;
using DbAdvance.Host.Commands.Setup.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Setup.Pipeline
{
    public sealed class SetupDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        public SetupDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<InspectSchemaStep>());

            RecordProcessingSteps(
                ResolveStep<CreateSchemaTablesStep>());
        }
    }
}