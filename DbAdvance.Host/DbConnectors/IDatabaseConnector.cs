namespace DbAdvance.Host.DbConnectors
{
    public interface IDatabaseConnector
    {
        void Apply(Step step);

        string GetDatabaseVersion();

        string GetBaseDatabaseVersion();

        void SetBaseDatabaseVersion(string version);
    }
}