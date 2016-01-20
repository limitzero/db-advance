using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;

namespace DbAdvance.Host.Commands.Commit.Pipeline.Steps
{
    public class GetInstallScriptsStep : GetPendingScriptsStep
    {
        public GetInstallScriptsStep(IKernel kernel, IFileSystem fileSystem) : base(kernel, fileSystem)
        {
            UseRollbackScripts = false;
        }
    }
}