using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Commands.Commit.Pipeline.Steps
{
    public sealed class UpgradeStep : ApplyScriptsStep
    {
        public UpgradeStep(IKernel kernel, DatabaseConnectorFactory factory) : base(kernel, factory)
        {
            UseRollbackScripts = false;
        }
    }

}