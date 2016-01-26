using System;
using System.IO;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Pack.Pipeline.Steps
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

            if (string.IsNullOrEmpty(context.Options.ScriptsPath))
            {
                context.Options.ScriptsPath = Environment.CurrentDirectory;
                Logger.WarnFormat(string.Concat(
                    "No path option supplied for the location of the scripts to be packaged, ",
                    "using the default directory of '{0}' as the starting point for packaging.."),
                    context.Options.ScriptsPath);
            }

            if (string.IsNullOrEmpty(context.Options.PackageDirectory))
            {
                context.Options.PackageDirectory = Path.Combine(Environment.CurrentDirectory, "packages");

                if (!Directory.Exists(context.Options.PackageDirectory))
                    Directory.CreateDirectory(context.Options.PackageDirectory);

                Logger.WarnFormat(string.Concat("No package directory stated for deployment, ",
                  "using generated package directory as '{0}'..."),
                  context.Options.PackageDirectory);
            }

            if (string.IsNullOrEmpty(context.Options.PackageFileName))
            {
                var package = context.Options.GetDefaultPackageName();
                context.Options.PackageFileName = package;

                Logger.WarnFormat(string.Concat("No package name stated for deployment, ",
                    "using generated package name as '{0}' file in the configured directory '{1}'..."),
                    context.Options.PackageFileName,
                    context.Options.ScriptsPath);
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