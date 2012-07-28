namespace DbAdvance.Host
{
    public class Configuration : IDatabaseConnectorConfiguration
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}