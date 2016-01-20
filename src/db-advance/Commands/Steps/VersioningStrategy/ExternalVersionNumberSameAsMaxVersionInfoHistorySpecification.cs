using System;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    public sealed class ExternalVersionNumberSameAsMaxVersionInfoHistorySpecification
        : BaseVersionDatabaseSpecification
    {
        public ExternalVersionNumberSameAsMaxVersionInfoHistorySpecification(
            IDatabaseConnectorConfiguration configuration) : base(configuration)
        {
        }

        public override bool IsMatch(string currentVersion, string desiredVersion)
        {
            return (!string.IsNullOrEmpty(currentVersion) & !string.IsNullOrEmpty(desiredVersion))
                   && (string.Equals(currentVersion, desiredVersion, StringComparison.CurrentCultureIgnoreCase));
        }

        public override void ExecuteVersioningStrategy(string currentVersion, string desiredVersion)
        {
            FromVersion = currentVersion;
            ToVersion = desiredVersion;
        }
    }
}