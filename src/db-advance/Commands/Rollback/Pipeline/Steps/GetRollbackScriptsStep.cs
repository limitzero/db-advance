using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;

namespace DbAdvance.Host.Commands.Rollback.Pipeline.Steps
{
    public class GetRollbackScriptsStep : GetPendingScriptsStep
    {
        public GetRollbackScriptsStep(IKernel kernel, IFileSystem fileSystem) : base(kernel, fileSystem)
        {
            UseRollbackScripts = true;
        }
    }
}