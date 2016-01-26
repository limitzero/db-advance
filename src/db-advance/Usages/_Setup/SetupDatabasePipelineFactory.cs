using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages._Setup.Pipeline;

namespace DbAdvance.Host.Usages._Setup
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
            Aliases = SetupDatabasePipeline.CommandAliases.ToArray();
        }
    }
}