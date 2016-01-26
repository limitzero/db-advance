using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Archiver;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Pack.Pipeline.Steps
{
    public class CreateZipArchiveForScriptPathStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IFileSystem _fileSystem;

        public CreateZipArchiveForScriptPathStep(IKernel kernel,
            IFileSystem fileSystem) : base(kernel)
        {
            _fileSystem = fileSystem;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();
            Logger.Info("STAGE: Packaging scripts path as *.zip archive for deployment");
            CreateZipArchive(context);
            Logger.WriteBanner();
        }

        private void CreateZipArchive(CommandPipelineContext context)
        {
            var archiver = new ZipArchiver();

            if (!context.Options.PackageFileName.EndsWith(".zip"))
                context.Options.PackageFileName = string.Format("{0}.zip", context.Options.PackageFileName);

            var zip = Path.Combine(context.Options.PackageDirectory, context.Options.PackageFileName);

            Logger.InfoFormat("Creating package '{0}' as zip archive for path '{1}'...",
                Path.GetFileName(zip),
                context.Options.ScriptsPath);

            // create a working directory as a copy of the scripts path 
            // due to zip archiver not being able to access directory 
            // while it is zipping the contents:
            var workingDirectory = string.Format(@"{0}-working-{1}",
                context.Options.ScriptsPath, Guid.NewGuid().ToString("N"));

            _fileSystem.CopyFolderContents(context.Options.ScriptsPath, workingDirectory, true);

            var files = new HashSet<string>();
            _fileSystem.GetFilesInPath(files, workingDirectory);

            var zipItems = files
                .Where(file => Path.GetExtension(file) == ".sql")
                .Select(file => new ZipItem(file, Path.GetDirectoryName(file)))
                .ToList();

            archiver.Zip(zip, zipItems);

            _fileSystem.DeleteFolder(workingDirectory);

            Logger.InfoFormat("Package '{0}' created for path '{1}'.",
                Path.GetFileName(zip),
                context.Options.ScriptsPath);
        }
    }
}