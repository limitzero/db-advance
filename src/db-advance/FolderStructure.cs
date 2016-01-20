using System.Collections.Generic;
using System.Linq;
using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class FolderStructure
    {
        public static readonly string ScriptFileExtension = ".sql";
        public static readonly string EnvironmentScriptFileExtension = ".env.sql";
        public static readonly string RunBeforeAll = "RunBeforeAll";
        public static readonly string Up = "Up";
        public static readonly string Down = "Down";
        public static readonly string RunOneTime = "RunOneTime";
        public static readonly string RunAfterAll = "RunAfterAll";

        public static readonly IDictionary<int, string> Folders =
            new Dictionary<int, string>
            {
                // run scripts in this directory everytime before a change:
                {1, "RunBeforeAll"},

                // run scripts in this directory evertime after a change:
                {2, "RunAfterAll"},

                // run scripts in this directory, if they have not been run, just once:
                {3, "RunOneTime"},

                // run all scripts in this directory on all forward changes, but do not re-run if content is the same
                {4, Up},

                // run all scripts in this directory on all backward changes, but do not re-run if content is the same
                {5, Down}
            };

        public static IEnumerable<string> GetFoldersForScriptRun()
        {
            return FolderStructure
                .Folders
                .OrderBy(folder => folder.Key)
                .Select(folder => folder.Value)
                .ToList();
        }

        public static IEnumerable<ScriptAccessor> GetUpScripts(IDelta delta)
        {
            return GetScriptsInFolder(delta, Up);
        }

        public static IEnumerable<ScriptAccessor> GetDownScripts(IDelta delta)
        {
            return GetScriptsInFolder(delta, Down);
        }

        public static IEnumerable<ScriptAccessor> GetScriptsInFolder(IDelta deltas, string folder)
        {
            var scripts = deltas
                .Scripts
                .Where(script => script.GetFullPath().Contains(folder))
                .Select(script => script)
                .ToList();
            return scripts;
        }
    }
}