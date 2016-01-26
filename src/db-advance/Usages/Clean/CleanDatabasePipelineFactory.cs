using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Clean.Pipeline;

namespace DbAdvance.Host.Usages.Clean
{
    public sealed class CleanDatabasePipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (CleanDatabasePipeline); }
        }

        public CleanDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = CleanDatabasePipeline.CommandAliases.ToArray();
        }
    }
}