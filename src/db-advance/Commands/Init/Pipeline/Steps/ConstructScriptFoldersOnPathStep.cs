using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Init.Pipeline.Steps
{
    public class ConstructScriptFoldersOnPathStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public ConstructScriptFoldersOnPathStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Verifying specified path and constructing script folder structure");

            var valid = InspectPath(context);

            if (!valid)
                Pipeline.Halt = true;
            else
            {
                ConstructFoldersInPath(context);
            }

            Logger.WriteBanner();
        }

        private bool InspectPath(CommandPipelineContext context)
        {
            if (string.IsNullOrEmpty(context.Options.Path))
            {
                Logger.WarnFormat("The path to initialize the folder structure was not supplied.");
                return false;
            }

            if (!Directory.Exists(context.Options.Path))
            {
                Logger.WarnFormat(
                    "The directory '{0}' does not exist on the file system for constructing the script folders.",
                    context.Options.Path);
                return false;
            }

            return true;
        }

        private void ConstructFoldersInPath(CommandPipelineContext context)
        {
            var folders = FolderStructure.Folders.OrderBy(folder => folder.Key).ToList();

            foreach (var folder in folders)
            {
                var folderName = folder.Value;
                var folderPath = Path.Combine(context.Options.Path, folderName);

                if (Directory.Exists(folderPath)) continue;

                Logger.InfoFormat("Constructing folder '{0}' on path '{1}'...",
                    folderName, context.Options.Path);

                Directory.CreateDirectory(folderPath);

                Logger.InfoFormat("Folder '{0}' constructed on path '{1}'.",
                    folderName, context.Options.Path);
            }
        }
    }
}