using System.Collections.Generic;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands.Steps.FolderRunStrategy
{
    public sealed class RunOneTimeFolderScriptsRunSpecification
        : BaseRunScriptsForFolderSpecification
    {
        public override string Folder
        {
            get { return FolderStructure.RunAfterAll; }
        }

        public override void Execute(CommandPipelineContext context,
            IDatabaseConnector connector,
            IEnumerable<ScriptAccessor> scripts)
        {
            // Need to sort scripts out that have run before...
            base.Execute(context, connector, scripts);
        }
    }
}