using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Restore.Pipeline;

namespace DbAdvance.Host.Usages.Restore
{
    public class RestoreDatabasePipelineFactory : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (RestoreDatabasePipeline); }
        }

        public RestoreDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = RestoreDatabasePipeline.CommandAliases.ToArray();
        }
        
    }
}