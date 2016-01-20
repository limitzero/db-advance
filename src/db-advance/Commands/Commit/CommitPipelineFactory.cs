using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Commit.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Commit
{
    public sealed class CommitPipelineFactory :
        BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (CommitPipeline); }
        }

        public CommitPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new List<string> {"up", "commit"};
        }
    }
}