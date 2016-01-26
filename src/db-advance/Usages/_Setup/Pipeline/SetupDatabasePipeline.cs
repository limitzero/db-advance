using System.Collections.Generic;
using Castle.MicroKernel;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages._Setup.Pipeline.Steps;

namespace DbAdvance.Host.Usages._Setup.Pipeline
{
    public sealed class SetupDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "__steup__"
        };

        public SetupDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Inspect target database for runner schema objects");
            Logger.WriteBanner();
            base.Execute(context);
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