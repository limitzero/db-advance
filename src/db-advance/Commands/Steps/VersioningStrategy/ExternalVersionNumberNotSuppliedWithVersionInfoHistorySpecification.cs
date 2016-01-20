using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    /// <summary>
    /// External version number not supplied with history recorded, 
    /// create the new version history with maximum identifier of the new version info 
    /// plus one (Non-Deterministic Versioning).
    /// </summary>
    public sealed class ExternalVersionNumberNotSuppliedWithVersionInfoHistorySpecification
        : BaseVersionDatabaseSpecification
    {
        public ExternalVersionNumberNotSuppliedWithVersionInfoHistorySpecification(
            IDatabaseConnectorConfiguration configuration) : base(configuration)
        {
        }

        public override bool IsMatch(string currentVersion, string desiredVersion)
        {
            return !string.IsNullOrEmpty(currentVersion) &
                   string.IsNullOrEmpty(desiredVersion);
        }

        public override void ExecuteVersioningStrategy(string currentVersion, string desiredVersion)
        {
            var version = CreateNewVersion();
            version.Version = version.Id.ToString();
            UpdateVersionInfo(version);

            FromVersion = currentVersion;
            ToVersion = version.Version;
        }
    }
}