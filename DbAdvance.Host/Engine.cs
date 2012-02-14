using System.IO;

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

        public void CommitFromVersion(string packagePath, string version, string connectionString)
        {
            log.Log("Apply package {0} from version {1} on server {2} ...", packagePath, version, connectionString);

            Deploy(packagePath, new FromVersionDatabaseScriptsScanner(ScanMode.Commit, version), connectionString);
        }

        public void RollbackToVersion(string packagePath, string version, string connectionString)
        {
            log.Log("Rollback package {0} to version {1} on server {2} ...", packagePath, version, connectionString);

            Deploy(packagePath, new FromVersionDatabaseScriptsScanner(ScanMode.Rollback, version), connectionString);
        }

        private void Deploy(
            string packagePath, 
            IDatabaseScriptsScanner scriptsScanner, 
            string connectionString)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "DbDeployNet");

            fileSystem.CreateFolder(tempPath);
            fileSystem.DeleteFolderContents(tempPath);

            zipArchiver.Unzip(
                packagePath,
                tempPath);

            databaseScriptsExecutor.Run(
                tempPath,
                scriptsScanner,
                connectionString);
        }
    }
}