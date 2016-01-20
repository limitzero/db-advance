using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Deploy.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Deploy
{
    public class DeployPipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (DeployPipeline); }
        }

        public DeployPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new string[] {"deploy"};
        }
    }
}