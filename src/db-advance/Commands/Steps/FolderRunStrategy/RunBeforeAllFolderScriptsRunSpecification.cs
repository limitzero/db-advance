using System;

namespace DbAdvance.Host.Commands.Steps.FolderRunStrategy
{
    public sealed class RunBeforeAllFolderScriptsRunSpecification
        : BaseRunScriptsForFolderSpecification
    {
        public override string Folder
        {
            get { return FolderStructure.RunBeforeAll; }
        }
    }
}