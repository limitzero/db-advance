using System;
using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Pack.Pipeline.Steps
{
    public class InspectParametersForPackStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public InspectParametersForPackStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            if (string.IsNullOrEmpty(context.Options.Database))
                context.RecordError("The database name must be supplied for a deploy operation.");

            if (string.IsNullOrEmpty(context.Options.Path))
            {
                context.Options.Path = Environment.CurrentDirectory;
                Logger.WarnFormat(string.Concat(
                    "No path option supplied for the location of the scripts to be packaged, ",
                    "using the default directory of '{0}' as the starting point for packaging.."),
                    context.Options.Path);
            }

            if (string.IsNullOrEmpty(context.Options.PackageName))
            {
                var package = context.Options.GetDefaultPackageName();
                context.Options.PackageName = package;

                Logger.WarnFormat(string.Concat("No package name stated for deployment, ",
                    "using generated package name as '{0}' file in the configured directory '{1}'..."),
                    context.Options.PackageName,
                    context.Options.Path);
            }

            if (context.HasErrors())
            {
                Logger.Error("The following errors were encountered while inspected the options for packging:");
                Logger.Error(context.GetErrors());
                Pipeline.Halt = true;
            }

            context.ClearErrors();
        }
    }
}