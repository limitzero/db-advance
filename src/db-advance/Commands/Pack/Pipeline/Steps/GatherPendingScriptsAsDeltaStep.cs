using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;
using DbAdvance.Host.Package;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Pack.Pipeline.Steps
{
    [Obsolete]
    public class GatherPendingScriptsAsDeltaStep 
        : BasePipelineStep<CommandPipelineContext>
    {
        public GatherPendingScriptsAsDeltaStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Gathering all pending scripts for packaging");

            var delta = GatherAllPendingScriptsAsDelta(context);

            delta.Scripts.ForEach(script =>
                Logger.InfoFormat("Packaging script '{0}'..."));

            if (!delta.Scripts.Any())
            {
                HaltPipeline = true;
            }
            else
            {
                context.Deltas = new[] { delta };
            }
            
            Logger.WriteBanner();
        }

        private IDelta GatherAllPendingScriptsAsDelta(CommandPipelineContext context)
        {
            var deltas = GetAllScripts(context);
            
            var scripts = new List<ScriptAccessor>();
            deltas.ForEach(d => scripts.AddRange(d.Scripts.Distinct()));

            var delta = new Delta {Scripts = scripts};

            context.Deltas = new List<IDelta>();

            return delta;
        }

        private IEnumerable<IDelta> GetAllScripts(CommandPipelineContext context)
        {
            var pendingScriptsStep = Pipeline.ResolveStep<GetPendingScriptsStep>();
            pendingScriptsStep.Execute(context);
            return context.Deltas;
        }
    }
}