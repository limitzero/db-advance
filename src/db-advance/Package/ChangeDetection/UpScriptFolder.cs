using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Package.ChangeDetection
{
    public class UpScriptFolder : BaseScriptFolder
    {
        public override string Folder
        {
            get { return FolderStructure.Up; }
        }

        public UpScriptFolder(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration)
            : base(kernel, fileSystem, configuration)
        {
        }

        public override IEnumerable<ScriptAccessor> Examine()
        {
            var scripts = base.Examine();
            var executedBefore = base.GetAllScriptsThatHaveBeenExecutedPreviously(scripts);
            var changed = GetAllScriptsThatHaveChangedSincePreviousExecution(scripts);

            if (executedBefore.Any())
            {
                if (changed.Any())
                    return changed;
                else
                {
                    scripts = scripts
                        .Except(executedBefore)
                        .ToList();
                }
            }

            return scripts;
        }
    }
}