using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Rebuild.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rebuild
{
    public class RebuildPipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (RebuildPipeline); }
        }

        public RebuildPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new[] {"rebuild", "refresh"};
        }
    }
}