using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._02_Setup.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._02_Setup
{
    public sealed class SetupDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        public SetupDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE - SETUP:  Backup & Create,  Drop & Restore");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {      
            RecordProcessingSteps(
                ResolveStep<CreateDatabaseStep>(), 
                ResolveStep<CreateInfoTablesStep>()
                );
        }
    }
}