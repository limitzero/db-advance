using System;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Backup.Pipeline;

namespace DbAdvance.Host.Usages.Backup
{
    public class BackupDatabasePipelineFactory : BasePipelineFactory<CommandPipelineContext>
    {
        public override Type PipelineType
        {
            get { return typeof (BackupDatabasePipeline); }
        }

        public BackupDatabasePipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = BackupDatabasePipeline.CommandAliases.ToArray();
        }
        
    }
}