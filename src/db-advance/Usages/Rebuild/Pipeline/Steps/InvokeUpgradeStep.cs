using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages;

namespace DbAdvance.Host.Usages.Rebuild.Pipeline.Steps
{
    public class InvokeUpgradeStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public InvokeUpgradeStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            var factory = Kernel.Resolve<CommandPipelineFactoryConnector>();
            context.Options.Command = UpPipeline.CommandAliases.First();
            factory.Apply(context);
        }
    }
}