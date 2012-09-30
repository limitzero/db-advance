namespace DbAdvance.Host.Commands
{
    public class PrintUsageCommand : ICommand
    {
        private readonly Logger logger;

        public PrintUsageCommand(Logger logger)
        {
            this.logger = logger;
        }

        public void Execute()
        {
            const string Usage = @"
Db Advance

Set Base Version:
DbAdvance.Host.exe -setbaseversion connectionString databaseName version

Commit Package to the latest version:
DbAdvance.Host.exe -commit connectionString databaseName packagePath

Commit Package to the specified version:
DbAdvance.Host.exe -committoversion connectionString databaseName packagePath version

Rollback Package to Base Version:
DbAdvance.Host.exe -rollback connectionString databaseName packagePath

Rollback Package to specified version:
DbAdvance.Host.exe -rollbacktoversion connectionString databaseName packagePath version
";

            logger.Log(Usage);
        }
    }
}