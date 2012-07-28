using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.Package
{
    public class PackageReader
    {
        public IEnumerable<IDelta> GetDeltas(string packageRootPath)
        {
            return Directory
                .EnumerateDirectories(packageRootPath)
                .Select(d => new Delta
                    {
                        CommitScripts = GetDeltaContents(d, true),
                        RollbackScripts = GetDeltaContents(d, false),
                        Version = Path.GetFileName(d)
                    })
                .OrderBy(d => d.Version)
                .ToList();
        }

        private static IEnumerable<ScriptAccessor> GetDeltaContents(string deltaPath, bool isCommit)
        {
            return Directory
                .EnumerateFiles(Path.Combine(deltaPath, isCommit ? "Commit" : "Rollback"), "*.sql")
                .OrderBy(fileName => fileName)
                .Select(fileName => new ScriptAccessor(fileName))
                .ToList();
        }
    }
}