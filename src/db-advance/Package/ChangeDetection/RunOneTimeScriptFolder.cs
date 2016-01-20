using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Package.ChangeDetection
{
    public class RunOneTimeScriptFolder : BaseScriptFolder
    {
        public override string Folder
        {
            get { return FolderStructure.RunOneTime; }
        }

        public RunOneTimeScriptFolder(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration)
            : base(kernel, fileSystem, configuration)
        {
        }

        public override IEnumerable<ScriptAccessor> Examine()
        {
            var scripts = base.Examine();
            var changed = GetAllScriptsThatHaveChangedSincePreviousExecution(scripts);

            if (!changed.Any())
            {
                return scripts;
            }

            return new List<ScriptAccessor>();
        }
    }
}