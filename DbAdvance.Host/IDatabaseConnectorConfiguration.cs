namespace DbAdvance.Host
{
    public interface IDatabaseConnectorConfiguration
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}