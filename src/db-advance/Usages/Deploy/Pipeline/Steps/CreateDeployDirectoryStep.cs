using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Archiver;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Init.Pipeline.Steps;

namespace DbAdvance.Host.Usages.Deploy.Pipeline.Steps
{
    public class PrepareDeploymentStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IFileSystem _fileSystem;

        public PrepareDeploymentStep(IKernel kernel, IFileSystem fileSystem) : base(kernel)
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
                Path.GetFileNameWithoutExtension(context.Options.PackageFileName);

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

            // the first file in the zip archive should be the root of the scripts 
            // directory as defined by the caller, move all of the content under 
            // this root folder to the root of the folder named after the package (i.e. *.zip file)
            // for the current deployment:
            var zipFileRootFolder = foldersInDeployDirectory.FirstOrDefault();
            _fileSystem.CopyFolderContents(zipFileRootFolder, packageDeployDirectory, true);

            //var installPath =
            //    foldersInDeployDirectory
            //        .Where(folder => FolderStructure.Folders.Values.Any(v => folder.Contains(v)))
            //        .Select(folder => ScrubInstallDirectoryFromFolderStructureNames(folder))
            //        .FirstOrDefault();

            // move the archive to the root of the deploy folder:
            _fileSystem.CopyFile(zipFile,
                Path.Combine(packageDeployDirectory, Path.GetFileName(zipFile)));

            // remove the extracted root folder of the *.zip file while
            // after moving its underlying content to the root of the 
            // folder with the package name (it will have a "temp" prefix
            // attached to the specified directory where the scripts are located):
            var tempDirectoryCreatedOnExtraction = Directory
                .EnumerateDirectories(packageDeployDirectory)
                .FirstOrDefault(d => d.EndsWith("temp"));

            if(!string.IsNullOrEmpty(tempDirectoryCreatedOnExtraction))
            _fileSystem.DeleteFolder(tempDirectoryCreatedOnExtraction);

            //var foldersToClean = foldersInDeployDirectory
            //    .Select(directory => Path.GetDirectoryName(directory))
            //    .Where(directory => !FolderStructure.Folders.Values.Contains(directory))
            //    .Select(directory => Path.Combine(packageDeployDirectory, directory))
            //    .Distinct()
            //    .ToList();

            //foreach (var folder in foldersToClean)
            //{
            //    try
            //    {
            //        _fileSystem.DeleteFolder(folder);
            //    }
            //    catch
            //    {
            //    }
            //}

            Logger.InfoFormat("'{0}' unpacked into directory '{1}'",
                GetZipFileName(context), packageDeployDirectory);

            Logger.InfoFormat("Redirecting scripts path to deployed package directory '{0}'...", packageDeployDirectory);
            context.Options.ScriptsPath = packageDeployDirectory;
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
            if (File.Exists(context.Options.PackageFileName))
                zipFileName = Path.GetFileNameWithoutExtension(context.Options.PackageFileName);
            else
            {
                zipFileName = context.Options.PackageFileName.Replace(".zip", string.Empty);
            }

            return zipFileName;
        }

        private string GetZipFileLocation(CommandPipelineContext context)
        {
            var zipFile = string.Empty;

            if (File.Exists(context.Options.PackageFileName))
                zipFile = context.Options.PackageFileName;
            else
            {
                zipFile = Path.Combine(Environment.CurrentDirectory, string.Concat(GetZipFileName(context), ".zip"));
            }

            return zipFile;
        }
    }
}