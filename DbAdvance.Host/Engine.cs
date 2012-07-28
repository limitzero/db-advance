using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class Engine
    {
        private readonly ILogger log;

        private readonly ZipArchiver zipArchiver;

        private readonly IFileSystem fileSystem;

        private readonly DatabaseConnectorFactory databaseConnectorFactory;

        private readonly PackageReader scriptsScanner;

        public Engine(
            ILogger log,
            ZipArchiver zipArchiver,
            IFileSystem fileSystem,
            DatabaseConnectorFactory databaseConnectorFactory,
            PackageReader scriptsScanner)
        {
            this.log = log;
            this.zipArchiver = zipArchiver;
            this.fileSystem = fileSystem;
            this.databaseConnectorFactory = databaseConnectorFactory;
            this.scriptsScanner = scriptsScanner;
        }

        public void RollbackAndCommit(string rollbackPath, string commitPath)
        {
            Rollback(commitPath);

            Commit(commitPath);
        }

        public void Commit(string packagePath)
        {
            var package = GetPackage(packagePath);

            using (var connector = databaseConnectorFactory.Create())
            {
                connector.Open();

                var databaseVersion = connector.GetDatabaseVersion();

                GetFrom(package)
                    .Zip(package, (fromVersion, toObject) => new Step { FromVersion = fromVersion, ToVersion = toObject.Version, Scripts = toObject.CommitScripts })
                    .OrderBy(d => d.FromVersion)
                    .SkipWhile(d => d.FromVersion != databaseVersion)
                    .ToList()
                    .ForEach(connector.Apply);
            }
        }

        public void Rollback(string packagePath)
        {
            var package = GetPackage(packagePath);

            using (var connector = databaseConnectorFactory.Create())
            {
                connector.Open();

                var baseVersion = connector.GetBaseDatabaseVersion();

                GetFrom(package)
                    .Zip(package, (fromVersion, toObject) => new Step { FromVersion = toObject.Version, ToVersion = fromVersion, Scripts = toObject.RollbackScripts })
                    .OrderBy(d => d.ToVersion)
                    .SkipWhile(d => d.ToVersion != baseVersion)
                    .Reverse()
                    .ToList()
                    .ForEach(connector.Apply);
            }
        }

        public void ReportVersions()
        {
            using (var connector = databaseConnectorFactory.Create())
            {
                connector.Open();

                var databaseVersion = connector.GetDatabaseVersion();
                var baseVersion = connector.GetBaseDatabaseVersion();

                log.Log("Database version is '{0}'", databaseVersion);
                log.Log("Base version is '{0}'", baseVersion);
            }
        }

        private IEnumerable<IDelta> GetPackage(string packagePath)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "DbAdvanceNet");

            fileSystem.CreateFolder(tempPath);
            fileSystem.DeleteFolderContents(tempPath);
            zipArchiver.Unzip(packagePath, tempPath);

            return scriptsScanner.GetDeltas(tempPath);
        }

        private static IEnumerable<string> GetFrom(IEnumerable<IDelta> deltas)
        {
            return deltas.Select(d => d.Version).Reverse().Skip(1).Union(new[] { (string)null }).Reverse();
        }
    }
}