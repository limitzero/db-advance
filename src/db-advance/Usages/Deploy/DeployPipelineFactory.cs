using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Deploy.Pipeline;

namespace DbAdvance.Host.Usages.Deploy
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
            Aliases = DeployPipeline.CommandAliases.ToArray();
        }
    }
}