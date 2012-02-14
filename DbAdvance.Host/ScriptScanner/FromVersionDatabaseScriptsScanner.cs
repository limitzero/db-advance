using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.ScriptScanner
{
    public class FromVersionDatabaseScriptsScanner : IDatabaseScriptsScanner
    {
        private readonly bool isCommit;
        private readonly string version;

        public FromVersionDatabaseScriptsScanner(ScanMode scanMode, string version)
        {
            isCommit = scanMode == ScanMode.Commit;
            this.version = version;
        }

        public List<DatabaseScriptVersion> GetScripts(string packageRootPath)
        {
            var directories = Directory.EnumerateDirectories(packageRootPath);

            var filteredDirectories = Filter(directories);

            var sorted = isCommit
                ? filteredDirectories.OrderBy(d => d)
                : filteredDirectories.OrderByDescending(d => d);

            return sorted
                .Select(d => new DatabaseScriptVersion
                    {
                        Path = d,
                        IsCommit = isCommit
                    })
                .ToList();
        }

        public IEnumerable<string> Filter(IEnumerable<string> enumerateDirectories)
        {
            return enumerateDirectories
                .Select(d => new
                    {
                        Path = d,
                        Name = Path.GetFileName(d)
                    })
                .OrderByDescending(d => d.Name)
                .TakeWhile(d => d.Name != version)
                .Select(d => d.Path);
        }
    }
}