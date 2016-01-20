using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rebuild.Pipeline.Steps
{
    public class BuildInfoTablesStep
        : BasePipelineStep<CommandPipelineContext>
    {
        public BuildInfoTablesStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            var factory = Kernel.Resolve<CommandPipelineFactoryConnector>();
            context.Options.ConfigureForSetup();
            factory.Apply(context);
        }
    }
}