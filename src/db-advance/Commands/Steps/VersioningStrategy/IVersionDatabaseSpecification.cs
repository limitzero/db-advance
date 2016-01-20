namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    public interface IVersionDatabaseSpecification
    {
        bool IsMatch(string currentVersion, string desiredVersion);
        void Execute(string currentVersion, string desiredVersion);
    }
}