using System;

namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnector : IDisposable
    {
        void Apply(Step step);

        void Open();

        string GetDatabaseVersion();

        string GetBaseDatabaseVersion();

        void SetBaseDatabaseVersion(string version);
    }
}