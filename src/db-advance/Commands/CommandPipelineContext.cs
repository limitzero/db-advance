using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Internal;
using DbAdvance.Host.Models;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Package;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages;

namespace DbAdvance.Host.Commands
{
    /// <summary>
    /// Root context for all commands for the pipeline
    /// </summary>
    public class CommandPipelineContext : BasePipelineContext
    {
        private IList<string> _errors = new List<string>();
        private List<ScriptsRunInfo> _allScriptsRun = new List<ScriptsRunInfo>();
        private List<ScriptsRunErrorInfo> _allScriptErrors = new List<ScriptsRunErrorInfo>();

        /// <summary>
        /// Gets or sets the flag as to whether the necesary history tables are 
        /// present in the target database. If they are not present, then they 
        /// will be created before each command is issued.
        /// </summary>
        public bool IsSchemaPresent { get; set; }

        /// <summary>
        /// Gets or sets the current version from target database.
        /// </summary>
        public string FromVersion { get; set; }

        /// <summary>
        /// Gets or sets the desired version to set after the changes are applied.
        /// </summary>
        public string ToVersion { get; set; }

        public DbAdvancedOptions Options { get; set; }

        public HashSet<ScriptAccessor> EnvironmentScriptsFound { get; set; } 

        /// <summary>
        /// Gets or sets the set of all changes from a given folder over a script execution run.
        /// </summary>
        public IEnumerable<IDelta> FolderDeltas { get; set; }

        /// <summary>
        /// Gets the accumulated set of all changes from all folders for a given script execution run.
        /// </summary>
        public IEnumerable<ScriptsRunInfo> AllScriptsRun
        {
            get { return _allScriptsRun; }
        }

        public IEnumerable<ScriptsRunErrorInfo> AllScriptErrors
        {
            get { return _allScriptErrors; }
        }

        public CommandPipelineContext()
        {
            FromVersion = "0";
        }

        public void RecordScriptInfoRun(ScriptsRunInfo info)
        {
            if (!_allScriptsRun.Any(s => s.ScriptName == info.ScriptName))
                _allScriptsRun.Add(info);
        }

        public void RecordScriptInfoRunError(ScriptsRunErrorInfo info)
        {
            if (!_allScriptErrors.Any(s => s.ScriptName == info.ScriptName))
                _allScriptErrors.Add(info);
        }

        public bool HasErrors()
        {
            return _errors.Any();
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        public void RecordError(string error)
        {
            _errors.Add(error);
        }

        public string GetErrors()
        {
            var builder = new StringBuilder();
            _errors.ForEach(error => builder.AppendFormat("- {0}", error).AppendLine());
            return builder.ToString();
        }
    }
}