using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Create.Pipeline;

namespace DbAdvance.Host.Usages.Create
{
    public sealed class CreateDatabasePipelineFactory
        : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (CreateDatabasePipeline); }
        }

        public CreateDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = CreateDatabasePipeline.CommandAliases.ToArray();
        }
    }
}