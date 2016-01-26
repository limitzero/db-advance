using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._04_Migrate.Steps
{
    public class ValidateMigrationParametersStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public ValidateMigrationParametersStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            if (string.IsNullOrEmpty(context.Options.ScriptsPath))
            {
                context.RecordError("There was not a path defined for locating the scripts to run.");
            }

            if (context.HasErrors())
            {
                Logger.Error("The following errors were encountered while inspected the options for migration:");
                Logger.Error(context.GetErrors());
                Pipeline.Halt = true;
            }

            context.ClearErrors();
        }
    }
}