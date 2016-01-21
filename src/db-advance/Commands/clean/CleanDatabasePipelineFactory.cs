using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Clean.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Clean
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
            Aliases = new[] {"clean"};
        }
    }
}