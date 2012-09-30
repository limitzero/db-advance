using System.Collections.Generic;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands
{
    public class RollbackCommand : PackageCommand
    {
        public RollbackCommand(ZipArchiver zipArchiver, IFileSystem fileSystem, PackageReader scriptsScanner, ILogger log, DatabaseConnectorFactory databaseConnectorFactory)
            : base(zipArchiver, fileSystem, scriptsScanner, log, databaseConnectorFactory)
        {
        }

        protected override IEnumerable<Step> GetSteps(IEnumerable<IDelta> package, string databaseVersion, string baseVersion)
        {
            CheckVersion(baseVersion, package);

            return GetRollbackSteps(package)
                .SkipWhile(d => d.ToVersion != baseVersion)
                .TakeWhile(d => string.Compare(d.FromVersion, databaseVersion, System.StringComparison.Ordinal) <= 0)
                .Reverse();
        }
    }
}