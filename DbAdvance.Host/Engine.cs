using System.Collections.Generic;
using System.IO;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.ScriptScanner;

namespace DbAdvance.Host
{
    public class Engine
    {
        private readonly ILogger log;
        private readonly ZipArchiver zipArchiver;
        private readonly IFileSystem fileSystem;
        private readonly DatabaseScriptsExecutor databaseScriptsExecutor;

        public Engine(
            ILogger log, 
            ZipArchiver zipArchiver, 
            IFileSystem fileSystem, 
            DatabaseScriptsExecutor databaseScriptsExecutor)
        {
            this.log = log;
            this.zipArchiver = zipArchiver;
            this.fileSystem = fileSystem;
            this.databaseScriptsExecutor = databaseScriptsExecutor;
        }

        public void CommitFromVersion(string packagePath, string version, string connectionString, string databaseName)
        {
            log.Log("Apply package {0} from version {1} on server {2} ...", packagePath, version, connectionString);

            Deploy(packagePath, new FromVersionDatabaseScriptsScanner(), connectionString, true, version, databaseName);
        }

        public void RollbackToVersion(string packagePath, string version, string connectionString, string databaseName)
        {
            log.Log("Rollback package {0} to version {1} on server {2} ...", packagePath, version, connectionString);

            Deploy(packagePath, new FromVersionDatabaseScriptsScanner(), connectionString, false, version, databaseName);
        }

        private void Deploy(string packagePath, IDatabaseScriptsScanner scriptsScanner, string connectionString, bool isCommit, string version, string databaseName)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "DbAdvanceNet");

            fileSystem.CreateFolder(tempPath);
            fileSystem.DeleteFolderContents(tempPath);

            zipArchiver.Unzip(
                packagePath,
                tempPath);

            Run(
                tempPath,
                scriptsScanner,
                connectionString,
                isCommit,
                version,
                databaseName);
        }

        private void Run(string packageRootPath, IDatabaseScriptsScanner scanner, string connectionString, bool isCommit, string fromVersion, string databaseName)
        {
            if (isCommit)
            {
                scanner
                    .GetScripts(packageRootPath)
                    .OrderBy(d => d.FromVersion)
                    .SkipWhile(d => d.FromVersion != fromVersion)
                    .ToList()
                    .ForEach(d => databaseScriptsExecutor.ApplyDelta(databaseName, connectionString, d.FromVersion, d.ToVersion, d.CommitScripts));
            }
            else
            {
                scanner
                    .GetScripts(packageRootPath)
                    .OrderBy(d => d.FromVersion)
                    .SkipWhile(d => d.FromVersion != fromVersion)
                    .Reverse()
                    .ToList()
                    .ForEach(d => databaseScriptsExecutor.ApplyDelta(databaseName, connectionString, d.ToVersion, d.FromVersion, d.RollbackScripts));
            }
        }
    }
}