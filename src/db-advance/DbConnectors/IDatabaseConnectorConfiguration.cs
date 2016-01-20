using System.Data.SqlClient;

namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnectorConfiguration
    {
        string ConnectionString { get; }

        string DatabaseName { get; }

        string DbConnectorType { get; }

        void Configure(DbAdvanceCommandLineOptions options);
        string GetDatabaseName();
        string GetDatabaseServerName();
        SqlConnection GetConnection();
        SqlConnection GetConnectionToMaster();
    }
}