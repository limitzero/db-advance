using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    /// <summary>
    /// External version number not supplied with no history recorded, 
    /// create the version history with identifier of the new version info 
    /// record created (Non-Deterministic Versioning).
    /// </summary>
    public sealed class ExternalVersionNumberNotSuppliedAndNoVersionInfoHistorySpecification
        : BaseVersionDatabaseSpecification
    {
        public ExternalVersionNumberNotSuppliedAndNoVersionInfoHistorySpecification(
            IDatabaseConnectorConfiguration configuration) : base(configuration)
        {
        }

        public override bool IsMatch(string currentVersion, string desiredVersion)
        {
            return string.IsNullOrEmpty(currentVersion) &
                   string.IsNullOrEmpty(desiredVersion);
        }

        public override void ExecuteVersioningStrategy(string currentVersion, string desiredVersion)
        {
            base.Logger.InfoFormat("No version history recorded, using version number from current marker in version information table..");

            var version = CreateNewVersion();
            version.Version = version.Id.ToString();
            UpdateVersionInfo(version);

            FromVersion = "0";
            ToVersion = version.Version;
        }
    }
}