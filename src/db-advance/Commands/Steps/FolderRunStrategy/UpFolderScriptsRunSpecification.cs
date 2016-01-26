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
    }
}