using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Rollback.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rollback
{
    public sealed class RollbackPipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (RollbackPipeline); }
        }

        public RollbackPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new List<string> {"down", "rollback"};
        }
    }
}