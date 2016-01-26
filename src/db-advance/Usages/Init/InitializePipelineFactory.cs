using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Init.Pipeline;

namespace DbAdvance.Host.Usages.Init
{
    public class InitializePipelineFactory : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (InitializePipeline); }
        }

        public InitializePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = InitializePipeline.CommandAliases.ToArray();
        }
    }
}