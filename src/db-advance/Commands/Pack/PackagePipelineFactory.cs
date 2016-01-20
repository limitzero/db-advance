using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Pack.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Pack
{
    public sealed class PackagePipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (PackagePipeline); }
        }

        public PackagePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new string[] {"pack"};
        }
    }
}