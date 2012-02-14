using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.ScriptScanner
{
    public class IterativeDatabaseScriptsScanner : IDatabaseScriptsScanner
    {
        public List<DatabaseScriptVersion> GetScripts(string packageRootPath)
        {
            var directories = Directory
                .EnumerateDirectories(packageRootPath)
                .ToArray();

            return GetIterativeScriptList(directories);
        }

        public List<DatabaseScriptVersion> GetIterativeScriptList(string[] directories)
        {
            var scripts = new List<DatabaseScriptVersion>();

            scripts.AddRange(
                directories
                    .OrderBy(d => d)
                    .Select(d => CreateDatabaseScriptVersion(d, true)));

            for (var i = 0; i < directories.Length; i++)
            {
                var iteration = directories
                    .OrderByDescending(d => d)
                    .Take(i + 1)
                    .ToArray();

                scripts.AddRange(
                    iteration
                        .Select(d => CreateDatabaseScriptVersion(d, false)));

                scripts.AddRange(
                    iteration
                        .OrderBy(d => d)
                        .Select(d => CreateDatabaseScriptVersion(d, true)));
            }

            return scripts;
        }

        private static DatabaseScriptVersion CreateDatabaseScriptVersion(string path, bool isCommit)
        {
            return new DatabaseScriptVersion
                {
                    Path = path,
                    IsCommit = isCommit
                };
        }
    }
}