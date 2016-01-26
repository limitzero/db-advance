using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Pack.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Pack.Pipeline
{
    public class PackagePipeline : BasePipeline<CommandPipelineContext>
    {
        public static readonly IEnumerable<string> CommandAliases = new List<string>
        {
            "pack"
        };

        public PackagePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("-- -: Packaging Items For Deployment : ---");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<InspectParametersForPackStep>());

            RecordProcessingSteps(
                ResolveStep<CreateZipArchiveForScriptPathStep>());
        }
    }
}