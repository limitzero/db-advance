using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rebuild.Pipeline.Steps
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
            context.Options.ConfigureForUp();
            factory.Apply(context);
        }
    }
}