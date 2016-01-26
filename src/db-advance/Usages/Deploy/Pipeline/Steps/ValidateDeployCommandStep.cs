using System;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Deploy.Pipeline.Steps
{
    public class InspectParametersForDeployStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public InspectParametersForDeployStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
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
                Logger.WarnFormat(string.Concat("No package name stated for deployment, ",
                    "using the most recent *.zip file in the configured directory '{0}'..."),
                    context.Options.PackageDirectory);

                context.Options.PackageFileName =
                    Directory
                        .EnumerateFiles(context.Options.PackageDirectory, "*.zip")
                        .OrderByDescending(f => f)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(context.Options.PackageFileName))
            {
                Logger.ErrorFormat(
                    "No *.zip file could be found on the path '{0}' for deploying the changes to the target database.",
                    context.Options.ScriptsPath);
            }

            if (context.HasErrors())
            {
                Logger.Error("The following errors were encountered while inspected the options for a deploy:");
                Logger.Error(context.GetErrors());
                Pipeline.Halt = true;
            }

            context.ClearErrors();
        }
    }
}