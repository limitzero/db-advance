using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Archiver;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Pack.Pipeline.Steps
{
    [Obsolete]
    public class CreateZipArchiveForDeltaStep 
        : BasePipelineStep<CommandPipelineContext>
    {
        public CreateZipArchiveForDeltaStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Creating package as *.zip archive based on pending scripts");

            if (!context.Deltas.Any())
            {
                Logger.Info("No pending files found to package.");
                HaltPipeline = true;
            }
            else
            {
                CreateZipArchiveForDelta(context);
            }

            Logger.WriteBanner();
        }

        private void CreateZipArchiveForDelta(CommandPipelineContext context)
        {
            var archiver = new ZipArchiver();
            var delta = context.Deltas.First();
       
            var commits = delta.CommitScripts
                .Select(script => new ZipItem(script.GetFullPath(),
                    Path.GetDirectoryName(script.GetFullPath())))
                .ToList();

            var rollbacks = delta.RollbackScripts
                .Select(script => new ZipItem(script.GetFullPath(),
                      Path.GetDirectoryName(script.GetFullPath())))
                .ToList();

            var zipitems = new List<ZipItem>(commits);
            zipitems.AddRange(rollbacks);

            if (!context.Options.PackageName.EndsWith(".zip"))
                context.Options.PackageName = string.Format("{0}.zip", context.Options.PackageName);

            var zip = Path.Combine(Environment.CurrentDirectory, context.Options.PackageName);

            Logger.InfoFormat("Creating package '{0}' as zip archive at '{1}'...", 
                Path.GetFileName(zip), 
                Path.GetDirectoryName(zip));
            
            archiver.Zip(zip, zipitems);

            Logger.InfoFormat("Package '{0}'  created at '{1}'.",
                Path.GetFileName(zip),
                Path.GetDirectoryName(zip));
        }
    }
}