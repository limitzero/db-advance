namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnectorConfiguration
    {
        string ConnectionString { get; }

        string DatabaseName { get; }

        string DbConnectorType { get; }
    }
}