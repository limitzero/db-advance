using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.ScriptScanner
{
    public class AllDatabaseScriptsScanner : IDatabaseScriptsScanner
    {
        private readonly bool isCommit;

        public AllDatabaseScriptsScanner(ScanMode scanMode)
        {
            isCommit = scanMode == ScanMode.Commit;
        }

        public List<DatabaseScriptVersion> GetScripts(string packageRootPath)
        {
            var enumerateDirectories = Directory.EnumerateDirectories(packageRootPath);

            var sorted = isCommit
                ? enumerateDirectories.OrderBy(d => d)
                : enumerateDirectories.OrderByDescending(d => d);

            return sorted
                .Select(d => new DatabaseScriptVersion
                    {
                        Path = d,
                        IsCommit = isCommit
                    })
                .ToList();
        }
    }
}