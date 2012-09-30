using System.Collections.Generic;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands
{
    public class CommitToVersionCommand : PackageCommand
    {
        public CommitToVersionCommand(ZipArchiver zipArchiver, IFileSystem fileSystem, PackageReader scriptsScanner, ILogger log, DatabaseConnectorFactory databaseConnectorFactory)
            : base(zipArchiver, fileSystem, scriptsScanner, log, databaseConnectorFactory)
        {
        }

        public string Version { get; set; }

        protected override IEnumerable<Step> GetSteps(IEnumerable<IDelta> package, string databaseVersion, string baseVersion)
        {
            CheckVersion(Version, package);

            return GetCommitSteps(package)
                .SkipWhile(d => d.FromVersion != databaseVersion)
                .TakeWhile(d => string.Compare(d.ToVersion, Version, System.StringComparison.Ordinal) <= 0);
        }
    }
}