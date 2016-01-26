using System.Data.SqlClient;
using DbAdvance.Host.Usages;

namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnectorConfiguration
    {
        string ConnectionString { get; }

        string DatabaseName { get; }

        string DbConnectorType { get; }

        void Configure(DbAdvancedOptions options);
        string GetDatabaseName();
        string GetDatabaseServerName();
        SqlConnection GetConnection();
        SqlConnection GetConnectionToMaster();
    }
}