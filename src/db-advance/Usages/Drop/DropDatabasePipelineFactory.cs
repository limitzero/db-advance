using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Drop.Pipeline;

namespace DbAdvance.Host.Usages.Drop
{
    public sealed class DropDatabasePipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (DropDatabasePipeline); }
        }

        public DropDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = DropDatabasePipeline.CommandAliases.ToArray();
        }
    }
}