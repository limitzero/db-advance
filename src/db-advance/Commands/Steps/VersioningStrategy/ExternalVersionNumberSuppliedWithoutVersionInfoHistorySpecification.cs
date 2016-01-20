using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    /// <summary>
    /// External version number supplied with no history recorded, 
    /// create the version history with the supplied version number (Deterministic Versioning).
    /// </summary>
    public sealed class ExternalVersionNumberSuppliedWithoutVersionInfoHistorySpecification
        : BaseVersionDatabaseSpecification
    {
        public ExternalVersionNumberSuppliedWithoutVersionInfoHistorySpecification(
            IDatabaseConnectorConfiguration configuration)
            : base(configuration)
        {
        }

        public override bool IsMatch(string currentVersion, string desiredVersion)
        {
            return string.IsNullOrEmpty(currentVersion) &
                   !string.IsNullOrEmpty(desiredVersion);
        }

        public override void ExecuteVersioningStrategy(string currentVersion, string desiredVersion)
        {
            Logger.InfoFormat("No version history information found, creating history at version '{0}'..",
                desiredVersion);
            var version = new VersionInfo {Version = desiredVersion};
            CreateNewVersion(version);
            Logger.InfoFormat("Version '{0}' recorded.", desiredVersion);

            FromVersion = "0";
            ToVersion = version.Version;
        }
    }
}