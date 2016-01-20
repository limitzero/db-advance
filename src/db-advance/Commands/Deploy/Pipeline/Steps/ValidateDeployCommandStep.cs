using System;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Deploy.Pipeline.Steps
{
    public class InspectParametersForDeployStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public InspectParametersForDeployStep(IKernel kernel) : base(kernel)
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
                    "No path option supplied for locating package as *.zip file ",
                    "for deployment, switching to current directory '{0}'"),
                    Environment.CurrentDirectory);
            }

            if (string.IsNullOrEmpty(context.Options.PackageName))
            {
                Logger.WarnFormat(string.Concat("No package name stated for deployment, ",
                    "using the most recent *.zip file in the configured directory '{0}'..."),
                    context.Options.Path);

                context.Options.PackageName =
                    Directory
                        .EnumerateFiles(context.Options.Path, "*.zip")
                        .OrderByDescending(f => f)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(context.Options.PackageName))
            {
                Logger.ErrorFormat(
                    "No *.zip file could be found on the path '{0}' for deploying the changes to the target database.",
                    context.Options.Path);
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