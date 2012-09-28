using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host
{
    using System.Configuration;

    public class Configuration : IDatabaseConnectorConfiguration
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string DbConnectorType
        {
            get { return ConfigurationManager.AppSettings["DbConnectorType"]; }
        }
    }
}