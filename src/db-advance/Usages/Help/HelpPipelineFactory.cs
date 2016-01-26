using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Help.Pipeline;

namespace DbAdvance.Host.Usages.Help
{
    public sealed class HelpPipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (HelpPipeline); }
        }

        public HelpPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = HelpPipeline.CommandAliases.ToArray();
        }
    }
}