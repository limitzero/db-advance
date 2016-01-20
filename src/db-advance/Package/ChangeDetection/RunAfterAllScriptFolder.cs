using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Package.ChangeDetection
{
    public class RunAfterAllScriptFolder : BaseScriptFolder
    {
        public override string Folder
        {
            get { return FolderStructure.RunAfterAll; }
        }

        public RunAfterAllScriptFolder(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration)
            : base(kernel, fileSystem, configuration)
        {
        }
    }
}