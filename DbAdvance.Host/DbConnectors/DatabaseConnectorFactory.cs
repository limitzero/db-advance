namespace DbAdvance.Host.DbConnectors
{
    using System;

    public class DatabaseConnectorFactory
    {
        private readonly ILogger log;
        private readonly IDatabaseConnectorConfiguration config;

        public DatabaseConnectorFactory(ILogger log, IDatabaseConnectorConfiguration config)
        {
            this.log = log;
            this.config = config;
        }

        public IDatabaseConnector Create()
        {
            if (string.Equals(config.DbConnectorType, "SqlCmdDatabaseConnector", StringComparison.OrdinalIgnoreCase))
            {
                return new SqlCmdDatabaseConnector(new SqlCmdRunner(log), log, config);
            }
         
            return new DefaultDatabaseConnector(log, config);
        }
    }
}