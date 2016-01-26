using System;
using System.IO;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Backup.Pipeline.Steps
{
    public class BackupDatabaseStep : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DatabaseConnectorFactory _factory;

        public BackupDatabaseStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration,
            DatabaseConnectorFactory factory) : base(kernel)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
            CreateBackupDirectoryIfNotSpecified(context);
            BackUpDatabaseToSpecifiedDirectory(context);
        }

        private void CreateBackupDirectoryIfNotSpecified(CommandPipelineContext context)
        {
            if (string.IsNullOrEmpty(context.Options.BackupDirectory))
            {
                context.Options.BackupDirectory = Path.Combine(Environment.CurrentDirectory, "backups");

                Logger.InfoFormat("Backup directory not specified, using '{0}'...",
                context.Options.BackupDirectory);
            }

            if (!Directory.Exists(context.Options.BackupDirectory))
            {
                Logger.InfoFormat("Creating backup directory {0}...",
                    context.Options.BackupDirectory);

                Directory.CreateDirectory(context.Options.BackupDirectory);
            }
        }

        private void BackUpDatabaseToSpecifiedDirectory(CommandPipelineContext context)
        {
            var timestamp = DateTime.Now.ToString("hhmmss");
            var backupDirectory = context.Options.BackupDirectory;

            var backupFile = Path.Combine(backupDirectory,
                 string.Format("{0} - Full Backup - {1}.bak", timestamp, _configuration.GetDatabaseName()));

            var statement =
                  string.Format(
                      @"BACKUP DATABASE [{0}] TO DISK = N'{1}' WITH NOFORMAT, INIT, NAME = N'{0} - Full Backup - {2}', SKIP, NOREWIND, NOUNLOAD, STATS = 10",
                      _configuration.GetDatabaseName(), backupFile, timestamp);

            Logger.InfoFormat("Backing up database '{0}' to location '{1}'...",
                _configuration.GetDatabaseName(), backupFile);

            var connector = _factory.UseSqlCmdConnector();
            connector.Apply(statement);

            Logger.InfoFormat("Database '{0}' backed up to location '{1}' with restore set date/time stamp of '{2}'.",
               _configuration.GetDatabaseName(), backupDirectory, timestamp);
        }
    }
}