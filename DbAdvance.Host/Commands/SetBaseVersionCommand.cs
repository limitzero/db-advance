using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands
{
    public class SetBaseVersionCommand : ICommand
    {
        private readonly DatabaseConnectorFactory databaseConnectorFactory;

        public SetBaseVersionCommand(DatabaseConnectorFactory databaseConnectorFactory)
        {
            this.databaseConnectorFactory = databaseConnectorFactory;
        }

        public string Version { get; set; }

        public void Execute()
        {
            var connector = databaseConnectorFactory.Create();

            connector.SetBaseDatabaseVersion(Version);
        }
    }
}
