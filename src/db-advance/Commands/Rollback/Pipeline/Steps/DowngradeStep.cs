using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Rollback.Pipeline.Steps
{
    public sealed class DowngradeStep : ApplyScriptsStep
    {
        public DowngradeStep(IKernel kernel, DatabaseConnectorFactory factory) : base(kernel, factory)
        {
            UseRollbackScripts = true;
        }
    }
}