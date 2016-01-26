using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Usages;

namespace DbAdvance.Host.Package.ChangeDetection
{
    public abstract class BaseScriptFolder
    {
        public IKernel Kernel { get; private set; }
        public IFileSystem FileSystem { get; private set; }
        public IDatabaseConnectorConfiguration Configuration { get; private set; }
        public DbAdvancedOptions Options { get; set; }

        public abstract string Folder { get; }

        public virtual IEnumerable<ScriptAccessor> Examine()
        {
            var scripts = GetNonEnvironmentSpecificScriptsFromFolder();
            var environmentScripts = GetEnvironmentSpecificScriptsFromFolder();
            return scripts.Union(environmentScripts).ToList();
        }

        protected BaseScriptFolder(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration)
        {
            Kernel = kernel;
            FileSystem = fileSystem;
            Configuration = configuration;
        }

        protected IEnumerable<ScriptAccessor> GetNonEnvironmentSpecificScriptsFromFolder()
        {
            var path = Path.Combine(Options.ScriptsPath, Folder);
            var files = new HashSet<string>();
            FileSystem.GetFilesInPath(files, path, string.Format("*{0}", 
                FolderStructure.ScriptFileExtension));

            return files
                .Select(file => new ScriptAccessor(file))
                .Where(file => !file.ToString().EndsWith(FolderStructure.EnvironmentScriptFileExtension))
                .Distinct()
                .ToList();
        }

        protected IEnumerable<ScriptAccessor> GetEnvironmentSpecificScriptsFromFolder()
        {
            if (string.IsNullOrEmpty(Options.Environment))
                return new List<ScriptAccessor>();

            var path = Path.Combine(Options.ScriptsPath, Folder);
            var environmentFiles = new HashSet<string>();
            FileSystem.GetFilesInPath(environmentFiles, path,
                string.Format("*{0}", FolderStructure.EnvironmentScriptFileExtension));

            return environmentFiles
                .Where(file => Path.GetFileName(file).StartsWith(string.Format("{0}.", Options.Environment)))
                .Select(file => new ScriptAccessor(file))
                .Distinct()
                .ToList();
        }

        protected IEnumerable<ScriptsRunInfo> GetExecutedScriptsFromHistory()
        {
            var statement = string.Format(
                @"select scripts.ScriptName, scripts.ScriptHash 
                from [{0}] scripts
                group by scripts.ScriptName, scripts.ScriptHash",
                ScriptsRunInfo.GetTableName());

            using (var connection = Configuration.GetConnection())
            {
                return connection
                    .Query<ScriptsRunInfo>(statement)
                    .Select(si => new ScriptsRunInfo
                    {
                        ScriptName = si.ScriptName,
                        ScriptHash = si.ScriptHash
                    })
                    .Distinct()
                    .ToList();
            }
        }

        protected IEnumerable<ScriptAccessor> GetAllScriptsThatHaveBeenExecutedPreviously(
            IEnumerable<ScriptAccessor> foundScripts)
        {
            var executedScripts = GetExecutedScriptsFromHistory();

            var executed = (from foundScript in foundScripts
                from executedScript in executedScripts
                let foundScriptAsRunnableScript = new ScriptsRunInfo
                {
                    ScriptText = foundScript.Read(),
                    ScriptName = foundScript.ToString()
                }
                where foundScriptAsRunnableScript.ScriptName == executedScript.ScriptName
                select foundScript)
                .Distinct()
                .ToList();

            return executed;
        }

        protected IEnumerable<ScriptAccessor> GetAllScriptsThatHaveChangedSincePreviousExecution(
            IEnumerable<ScriptAccessor> foundScripts)
        {
            var foundScriptsAsScriptInfo = foundScripts
                .Select(info => new Tuple<string, ScriptsRunInfo>(info.GetFullPath(), new ScriptsRunInfo
                {
                    ScriptText = info.Read(),
                    ScriptName = info.ToString()
                }))
                .ToList();

            var executedPreviouslyAsScriptInfo =
                GetAllScriptsThatHaveBeenExecutedPreviously(foundScripts)
                    .Select(info => new Tuple<string, ScriptsRunInfo>(info.GetFullPath(), new ScriptsRunInfo
                    {
                        ScriptText = info.Read(),
                        ScriptName = info.ToString()
                    }))
                    .ToList();

            var changed = foundScriptsAsScriptInfo
                .Where(fs => executedPreviouslyAsScriptInfo.Any(es =>
                    (es.Item2.ScriptHash.ToString() != fs.Item2.ScriptHash.ToString())
                    & (es.Item2.ScriptName == fs.Item2.ScriptName)))
                .Select(s => new ScriptAccessor(s.Item1))
                .ToList();

            return changed;
        }

        public IEnumerable<IDelta> CreateDeltasFromScripts(IEnumerable<ScriptAccessor> scripts)
        {
            var deltas = new List<IDelta>();
            scripts.ForEach(script => deltas.Add(new Delta() {Scripts = new[] {script}}));
            return deltas.Distinct().ToList();
        }
    }
}