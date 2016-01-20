using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.Package
{
    public class PackageReader
    {
        public IEnumerable<IDelta> GetDeltasForPendingDirectory(string packageRootPath)
        {
            var delta = new Delta
            {
                //CommitScripts = GetDeltaContents(packageRootPath, true),
                //RollbackScripts = GetDeltaContents(packageRootPath, false),
                Tag = Path.GetFileName(packageRootPath)
            };

            return new Delta[] {delta};
        }


        public IEnumerable<IDelta> GetDeltas(string packageRootPath)
        {
            return Directory
                .EnumerateDirectories(packageRootPath)
                .Select(d => new Delta
                {
                    Scripts = GetDeltaContents(packageRootPath),
                    Tag =
                        FolderStructure.Folders.Values.Any(folder => folder == Path.GetFileName(d))
                            ? string.Empty
                            : Path.GetFileName(d)
                })
                .OrderBy(d => d.Tag)
                .ToList();
        }

        public IEnumerable<IDelta> GetDeltas2(string packageRootPath)
        {
            return Directory
                .EnumerateDirectories(packageRootPath)
                .Select(d => new Delta
                {
                    CommitScripts = GetDeltaContents2(d, true),
                    RollbackScripts = GetDeltaContents2(d, false),
                    Tag = Path.GetFileName(d)
                })
                .OrderBy(d => d.Tag)
                .ToList();
        }

        private static IEnumerable<ScriptAccessor> GetDeltaContents(string deltaPath)
        {
            return Directory
                .EnumerateFiles(Path.Combine(deltaPath), "*.sql")
                .OrderBy(fileName => fileName)
                .Select(fileName => new ScriptAccessor(fileName))
                .ToList();
        }

        private static IEnumerable<ScriptAccessor> GetDeltaContents2(string deltaPath, bool isCommit)
        {
            return Directory
                //.EnumerateFiles(Path.Combine(deltaPath, isCommit ? "Commit" : "Rollback"), "*.sql")
                .EnumerateFiles(Path.Combine(deltaPath, isCommit ? "Install" : "Rollback"), "*.sql")
                .OrderBy(fileName => fileName)
                .Select(fileName => new ScriptAccessor(fileName))
                .ToList();
        }
    }
}