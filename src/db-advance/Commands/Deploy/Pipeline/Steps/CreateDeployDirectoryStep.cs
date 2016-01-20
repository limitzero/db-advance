using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Archiver;
using DbAdvance.Host.Commands.Init.Pipeline.Steps;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Deploy.Pipeline.Steps
{
    public class CreateDeployDirectoryStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IFileSystem _fileSystem;

        public CreateDeployDirectoryStep(IKernel kernel, IFileSystem fileSystem) : base(kernel)
        {
            _fileSystem = fileSystem;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.InfoFormat("STAGE: Expand package into deployments directory");

            var deployDirectory = CreateDeploymentsDirectoryIfNeeded();
            var packageDirectory = CreateFolderAsPackageNameInDeploymentsFolder(context, deployDirectory);
            ExpandPackageIntoPackageFolderInDeploymentsDirectory(context, packageDirectory);

            Logger.WriteBanner();

            // make sure to have the full script structure in the package extraction directory:
            var step = Pipeline.ResolveStep<ConstructScriptFoldersOnPathStep>();
            step.Execute(context);
        }

        private string CreateDeploymentsDirectoryIfNeeded()
        {
            // look in the current directory where the executeable is located 
            // for the "Deployments" directory, if not there then create it:
            var deployDirectory = Path.Combine(Environment.CurrentDirectory, "Deployments");
            _fileSystem.CreateFolder(deployDirectory);
            return deployDirectory;
        }

        private string CreateFolderAsPackageNameInDeploymentsFolder(
            CommandPipelineContext context,
            string deployDirectory)
        {
            var packageName =
                Path.GetFileNameWithoutExtension(context.Options.PackageName);

            var packageDirectory = Path.Combine(deployDirectory, packageName);

            Logger.InfoFormat("Creating deployment directory '{0}' for package '{1}'...",
                packageDirectory, GetZipFileName(context));

            _fileSystem.CreateFolder(packageDirectory);

            Logger.InfoFormat("Deployment directory '{0}' created for package '{1}'.",
                packageDirectory, GetZipFileName(context));

            return packageDirectory;
        }

        private void ExpandPackageIntoPackageFolderInDeploymentsDirectory(
            CommandPipelineContext context,
            string packageDeployDirectory)
        {
            var zipArchiver = new ZipArchiver();
            var zipFile = GetZipFileLocation(context);

            Logger.InfoFormat("Unpacking '{0}' into directory '{1}'...",
                GetZipFileName(context), packageDeployDirectory);

            zipArchiver.Unzip(zipFile, packageDeployDirectory);

            // gather all of the directories in the archive 
            // path and look for a folder that matches the 
            // standard folder name, when we have this
            // we can move those contents under the root 
            // deploy path:
            var foldersInDeployDirectory = new HashSet<string>();
            _fileSystem.GetFoldersInPath(foldersInDeployDirectory, packageDeployDirectory);

            var installPath =
                foldersInDeployDirectory
                    .Where(folder => FolderStructure.Folders.Values.Any(v => folder.Contains(v)))
                    .Select(folder => ScrubInstallDirectoryFromFolderStructureNames(folder))
                    .FirstOrDefault();

            // move the items in the subdirectories as defined 
            // in the archive to the root deploy directory path:
            _fileSystem.CopyFolderContents(installPath, packageDeployDirectory, true);

            // move the archive to the root of the deploy folder:
            _fileSystem.CopyFile(zipFile,
                Path.Combine(packageDeployDirectory, Path.GetFileName(zipFile)));

            // clean the install directory for folders that do not have the defined folder names
            // as created from un-packing of zip archive:
            var foldersToClean = foldersInDeployDirectory
                .Select(directory => Path.GetDirectoryName(directory))
                .Where(directory => !FolderStructure.Folders.Values.Contains(directory))
                .Select(directory => Path.Combine(packageDeployDirectory, directory))
                .Distinct()
                .ToList();

            foreach (var folder in foldersToClean)
            {
                try
                {
                    _fileSystem.DeleteFolder(folder);
                }
                catch
                {
                }
            }


            Logger.InfoFormat("'{0}' unpacked into directory '{1}'",
                GetZipFileName(context), packageDeployDirectory);

            Logger.InfoFormat("Redirecting scripts path to deployed package directory '{0}'...", packageDeployDirectory);
            context.Options.Path = packageDeployDirectory;
        }

        private string ScrubInstallDirectoryFromFolderStructureNames(string directory)
        {
            var result = directory;
            foreach (var folder in FolderStructure.Folders.Values)
            {
                if (result.Contains(folder))
                    result = result.Replace(string.Concat(@"\", folder), string.Empty);
            }

            return result;
        }

        private string GetZipFileName(CommandPipelineContext context)
        {
            string zipFileName;
            if (File.Exists(context.Options.PackageName))
                zipFileName = Path.GetFileNameWithoutExtension(context.Options.PackageName);
            else
            {
                zipFileName = context.Options.PackageName.Replace(".zip", string.Empty);
            }

            return zipFileName;
        }

        private string GetZipFileLocation(CommandPipelineContext context)
        {
            var zipFile = string.Empty;

            if (File.Exists(context.Options.PackageName))
                zipFile = context.Options.PackageName;
            else
            {
                zipFile = Path.Combine(Environment.CurrentDirectory, string.Concat(GetZipFileName(context), ".zip"));
            }

            return zipFile;
        }
    }
}