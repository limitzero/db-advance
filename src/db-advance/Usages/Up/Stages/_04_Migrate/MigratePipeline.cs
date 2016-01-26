using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages._04_Migrate.Steps;

namespace DbAdvance.Host.Usages.Up.Stages._04_Migrate
{
    public class MigratePipeline : BasePipeline<CommandPipelineContext>
    {
        public MigratePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Migration");
            Logger.WriteBanner();
            base.Execute(context);
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<ValidateMigrationParametersStep>(),
                ResolveStep<RunBeforeAllStep>(),
                ResolveStep<RunOneTimeStep>());

            RecordProcessingSteps(
                ResolveStep<RunUpStep>());

            RecordPostProcessingSteps(
                ResolveStep<RunAfterAllStep>());
        }
    }
}