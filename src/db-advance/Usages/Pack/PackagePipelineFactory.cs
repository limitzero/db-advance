using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Pack.Pipeline;

namespace DbAdvance.Host.Usages.Pack
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
            Aliases = PackagePipeline.CommandAliases.ToArray();
        }
    }
}