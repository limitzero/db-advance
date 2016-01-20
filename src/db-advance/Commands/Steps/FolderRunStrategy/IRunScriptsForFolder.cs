using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands.Steps.FolderRunStrategy
{
    public interface IRunScriptsForFolderSpecification
    {
        bool IsMatch(string scriptPath);
        void Execute(CommandPipelineContext context, IDatabaseConnector connector, IEnumerable<ScriptAccessor> scripts);
    }

    public abstract class BaseRunScriptsForFolderSpecification
        : IRunScriptsForFolderSpecification
    {
        public ILogger Logger { get; set; }
        public abstract string Folder { get; }

        public bool IsMatch(string scriptPath)
        {
            return scriptPath.Contains(Folder);
        }

        public virtual void Execute(CommandPipelineContext context,
            IDatabaseConnector connector,
            IEnumerable<ScriptAccessor> scripts)
        {
            if (!scripts.Any()) return;
            var step = new Step {Scripts = scripts};

            Logger.InfoFormat("Executing scripts in folder '{0}'...", Folder);
            connector.Apply(step);
            Logger.InfoFormat("Scripts in folder '{0}' executed.", Folder);
        }
    }
}