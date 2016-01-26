using System;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Help.Pipeline.Steps
{
    public class ShowUseageStep : BasePipelineStep<CommandPipelineContext>
    {
        public ShowUseageStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            var resource = GetType().Assembly.GetManifestResourceNames()
                .FirstOrDefault(s => s.EndsWith("Useage.txt"));

            if (string.IsNullOrEmpty(resource)) return;

            using (var stream = GetType().Assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                if (string.IsNullOrEmpty(content)) return;

                Console.WriteLine(content);
                Console.Write("Press any key to exit...");
                Console.ReadKey();
            }

        }
    }
}