namespace DbAdvance.Host
{
    public class DatabaseConnectorFactory
    {
        private readonly ILogger log;
        private readonly IDatabaseConnectorConfiguration config;

        public DatabaseConnectorFactory(ILogger log, IDatabaseConnectorConfiguration config)
        {
            this.log = log;
            this.config = config;
        }

        public DatabaseConnector Create()
        {
            return new DatabaseConnector(log, config);
        }
    }
}