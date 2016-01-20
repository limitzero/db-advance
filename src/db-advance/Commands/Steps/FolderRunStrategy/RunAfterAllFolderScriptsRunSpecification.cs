namespace DbAdvance.Host.Commands.Steps.FolderRunStrategy
{
    public sealed class RunAfterAllFolderScriptsRunSpecification
        : BaseRunScriptsForFolderSpecification
    {
        public override string Folder
        {
            get { return FolderStructure.RunAfterAll; }
        }
    }
}