using System.Collections.Generic;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Commands.Steps.FolderRunStrategy
{
    public sealed class UpFolderScriptsRunSpecification
        : BaseRunScriptsForFolderSpecification
    {
        public override string Folder
        {
            get { return FolderStructure.Up; }
        }

        public override void Execute(CommandPipelineContext context,
            IDatabaseConnector connector,
            IEnumerable<ScriptAccessor> scripts)
        {
            if (context.Options.Install)
                base.Execute(context, connector, scripts);
        }
    }
}