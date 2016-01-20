using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Package.ChangeDetection
{
    public class RunBeforeAllScriptFolder : BaseScriptFolder
    {
        public override string Folder
        {
            get { return FolderStructure.RunBeforeAll; }
        }

        public RunBeforeAllScriptFolder(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration)
            : base(kernel, fileSystem, configuration)
        {
        }
    }
}