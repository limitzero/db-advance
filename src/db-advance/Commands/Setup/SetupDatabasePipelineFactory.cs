using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Setup.Pipeline;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Setup
{
    public sealed class SetupDatabasePipelineFactory :
        BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (SetupDatabasePipeline); }
        }

        public SetupDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new List<string> {"setup"};
        }
    }
}