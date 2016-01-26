using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Rebuild.Pipeline;

namespace DbAdvance.Host.Usages.Rebuild
{
    public class RebuildDatabasePipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (RebuildDatabasePipeline); }
        }

        public RebuildDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = RebuildDatabasePipeline.CommandAliases.ToArray();
        }


    }
}