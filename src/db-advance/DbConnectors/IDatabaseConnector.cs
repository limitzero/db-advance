using System;

namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnector
    {
        event Action<ScriptRunResult> OnScriptExecuted;

        void Apply(string statement);

        void Apply(Step step);

        string GetDatabaseVersion();

        string GetBaseDatabaseVersion();

        string GetDatabaseName();

        void SetBaseDatabaseVersion(string version);

        bool DatabaseExists();
    }
}