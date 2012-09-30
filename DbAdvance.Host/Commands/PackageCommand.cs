using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands
{
    public abstract class PackageCommand : ICommand
    {
        private readonly ZipArchiver zipArchiver;

        private readonly IFileSystem fileSystem;

        private readonly PackageReader scriptsScanner;

        private readonly DatabaseConnectorFactory databaseConnectorFactory;

        private readonly ILogger log;

        protected PackageCommand(ZipArchiver zipArchiver, IFileSystem fileSystem, PackageReader scriptsScanner, ILogger log, DatabaseConnectorFactory databaseConnectorFactory)
        {
            this.zipArchiver = zipArchiver;
            this.fileSystem = fileSystem;
            this.scriptsScanner = scriptsScanner;
            this.log = log;
            this.databaseConnectorFactory = databaseConnectorFactory;
        }

        public string PackagePath { get; set; }

        public void Execute()
        {
            var package = GetPackage(PackagePath);

            var connector = databaseConnectorFactory.Create();
            var databaseVersion = connector.GetDatabaseVersion();
            var baseVersion = connector.GetBaseDatabaseVersion();

            log.Log("Database version is '{0}'", databaseVersion);
            log.Log("Base version is '{0}'", baseVersion);

            GetSteps(package, databaseVersion, baseVersion)
                .ToList()
                .ForEach(connector.Apply);
        }

        protected abstract IEnumerable<Step> GetSteps(IEnumerable<IDelta> package, string databaseVersion, string baseVersion);

        protected static void CheckVersion(string version, IEnumerable<IDelta> package)
        {
            if (version == null)
            {
                return;
            }

            if (package.All(p => p.Version != version))
            {
                throw new Exception(string.Format("Package doesn't contain version {0}", version));
            }
        }

        protected IEnumerable<IDelta> GetPackage(string packagePath)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "DbAdvanceNet");

            fileSystem.CreateFolder(tempPath);
            fileSystem.DeleteFolderContents(tempPath);
            zipArchiver.Unzip(packagePath, tempPath);

            return scriptsScanner.GetDeltas(tempPath);
        }

        protected static IEnumerable<Step> GetRollbackSteps(IEnumerable<IDelta> package)
        {
            return GetVersions(package)
                .Zip(package, (fromVersion, toObject) => new Step
                {
                    FromVersion = toObject.Version,
                    ToVersion = fromVersion,
                    Scripts = toObject.RollbackScripts
                })
                .OrderBy(d => d.ToVersion);
        }

        protected static IEnumerable<Step> GetCommitSteps(IEnumerable<IDelta> package)
        {
            return GetVersions(package)
                .Zip(package, (fromVersion, toObject) => new Step
                {
                    FromVersion = fromVersion,
                    ToVersion = toObject.Version,
                    Scripts = toObject.CommitScripts
                })
                .OrderBy(d => d.FromVersion);
        }

        private static IEnumerable<string> GetVersions(IEnumerable<IDelta> deltas)
        {
            return deltas
                .Select(d => d.Version)
                .Reverse()
                .Skip(1)
                .Union(new[] { (string)null })
                .Reverse();
        }
    }
}