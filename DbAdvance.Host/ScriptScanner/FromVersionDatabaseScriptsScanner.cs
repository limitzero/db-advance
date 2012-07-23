using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DbAdvance.Host.ScriptScanner
{
    public class FromVersionDatabaseScriptsScanner : IDatabaseScriptsScanner
    {
        public IEnumerable<DatabaseScriptVersion> GetScripts(string packageRootPath)
        {
            var directories = Directory.EnumerateDirectories(packageRootPath);

            return GetDeltas(directories)
                .Select(d => new DatabaseScriptVersion
                    {
                        CommitScripts = GetDeltaContents(d.Item3, true),
                        RollbackScripts = GetDeltaContents(d.Item3, false),
                        FromVersion = d.Item1,
                        ToVersion = d.Item2
                    })
                .ToList();
        }

        private static IEnumerable<string> GetDeltaContents(string deltaPath, bool isCommit)
        {
            return
                Directory
                    .EnumerateFiles(Path.Combine(deltaPath, isCommit ? "Commit" : "Rollback"), "*.sql")
                    .OrderBy(fileName => fileName);
        }

        private static IEnumerable<Tuple<string, string, string>> GetDeltas(IEnumerable<string> directories)
        {
            var to = directories
                .Select(d => new
                    {
                        Path = d,
                        Name = Path.GetFileName(d)
                    })
                .OrderBy(d => d.Name);

            var from = to.Select(d => d.Name).Reverse().Skip(1).Union(new[] { (string)null }).Reverse();

            return from.Zip(to, (fromVersion, toObject) => new Tuple<string, string, string>(fromVersion, toObject.Name, toObject.Path));
        }
    }
}