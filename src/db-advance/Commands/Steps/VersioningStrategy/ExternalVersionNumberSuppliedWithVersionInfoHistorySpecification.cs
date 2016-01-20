using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    /// <summary>
    /// External version number supplied with  history recorded, 
    /// create the version history with the supplied version number (Deterministic Versioning).
    /// </summary>
    public sealed class ExternalVersionNumberSuppliedWithVersionInfoHistorySpecification
        : BaseVersionDatabaseSpecification
    {
        public ExternalVersionNumberSuppliedWithVersionInfoHistorySpecification(
            IDatabaseConnectorConfiguration configuration) : base(configuration)
        {
        }

        public override bool IsMatch(string currentVersion, string desiredVersion)
        {
            return !string.IsNullOrEmpty(currentVersion) &
                   !string.IsNullOrEmpty(desiredVersion);
        }

        public override void ExecuteVersioningStrategy(string currentVersion, string desiredVersion)
        {
            var version = CreateNewVersion();
            version.Version = desiredVersion;
            UpdateVersionInfo(version);

            FromVersion = currentVersion;
            ToVersion = desiredVersion;
        }
    }
}