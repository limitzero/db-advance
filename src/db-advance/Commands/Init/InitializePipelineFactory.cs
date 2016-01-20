using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Init.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Init
{
    public class InitializePipelineFactory : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (InitializePipeline); }
        }

        public InitializePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new string[] {"init", "initialize"};
        }
    }
}