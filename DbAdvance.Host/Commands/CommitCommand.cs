using System.Collections.Generic;
using System.Linq;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands
{
    public class CommitCommand : PackageCommand
    {
        public CommitCommand(ZipArchiver zipArchiver, IFileSystem fileSystem, PackageReader scriptsScanner, ILogger log, DatabaseConnectorFactory databaseConnectorFactory)
            : base(zipArchiver, fileSystem, scriptsScanner, log, databaseConnectorFactory)
        {
        }

        protected override IEnumerable<Step> GetSteps(IEnumerable<IDelta> package, string databaseVersion, string baseVersion)
        {
            return GetCommitSteps(package)
                .SkipWhile(d => d.FromVersion != databaseVersion);
        }
    }
}