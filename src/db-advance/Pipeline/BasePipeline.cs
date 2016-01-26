using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Logging;
using Castle.MicroKernel;

namespace DbAdvance.Host.Pipeline
{
    public abstract class BasePipeline<T> : IPipeline<T>
        where T : BasePipelineContext
    {
        private readonly IKernel _kernel;
        private List<IPipeline<T>> _pipelineChannels = new List<IPipeline<T>>();

        public ILogger Logger { get; private set; }
        public bool Halt { get; set; }
        public IEnumerable<IPipelineStep<T>> PreProcessingSteps { get; private set; }
        public IEnumerable<IPipelineStep<T>> ProcessingSteps { get; private set; }
        public IEnumerable<IPipelineStep<T>> PostProcessingSteps { get; private set; }

        public IEnumerable<IPipeline<T>> PipelineChannels
        {
            get { return _pipelineChannels; }          
        }

        protected BasePipeline(IKernel kernel)
        {
            _kernel = kernel;
            Logger = _kernel.Resolve<ILogger>();
        }

        public virtual void Configure()
        {
            IEnumerable<IPipelineStep<T>> noop = new List<IPipelineStep<T>>();
            PreProcessingSteps = noop;
            ProcessingSteps = noop;
            PostProcessingSteps = noop;
        }

        protected void RecordPipelineChannel<TChannel>()
            where TChannel : class, IPipeline<T>
        {
            var pipeline = _kernel.ResolveAll<IPipeline>()
                .FirstOrDefault(p => p.GetType() == typeof (TChannel))
                as TChannel;

            _pipelineChannels.Add(pipeline);
        }

        protected void RecordPreProcessingSteps(params IPipelineStep<T>[] steps)
        {
            PreProcessingSteps = steps;
        }

        protected void RecordProcessingSteps(params IPipelineStep<T>[] steps)
        {
            ProcessingSteps = steps;
        }

        protected void RecordPostProcessingSteps(params IPipelineStep<T>[] steps)
        {
            PostProcessingSteps = steps;
        }

        public IPipelineStep<T> ResolveStep<S>()
            where S : class, IPipelineStep<T>
        {
            var step = _kernel
                .ResolveAll<IPipelineStep>()
                .FirstOrDefault(s => typeof (S).IsAssignableFrom(s.GetType())) as IPipelineStep<T>;

            return step;
        }

        public virtual void ExecuteChannels(T context)
        {
            foreach (var pipelineChannel in _pipelineChannels)
            {
                pipelineChannel.Configure();
                pipelineChannel.Execute(context);

                if (pipelineChannel.Halt)
                {
                    Halt = true;
                    break;
                }
            }
        }

        public virtual void Execute(T context)
        {
            if(PipelineChannels.Any())
                ExecuteChannels(context);

            ExecuteSteps(PreProcessingSteps, context);
            ExecuteSteps(ProcessingSteps, context);
            ExecuteSteps(PostProcessingSteps, context);
        }

        public virtual void PreStepExecution(IPipelineStep<T> step, T context)
        {
        }

        public virtual void PostStepExecution(IPipelineStep<T> step, T context)
        {
        }

        private void ExecuteSteps(IEnumerable<IPipelineStep<T>> steps, T context)
        {
            if (steps == null || Halt) return;
            var logger = _kernel.Resolve<ILogger>();

            foreach (var step in steps)
            {
                if (step == null) continue;
                var stopwatch = new Stopwatch();
                var stepName = step.GetType().Name;

                try
                {
                    PreStepExecution(step, context);

                    if (Halt)
                        break;

                    step.Pipeline = this;
                    //WriteInformationForStep(stepName, step, context);
                    step.Execute(context);

                    if (Halt)
                        break;

                    PostStepExecution(step, context);

                    if (Halt)
                        break;
                }
                catch
                {
                    throw;
                }
            }
        }

        private void WriteInformationForStep(string stepName, IPipelineStep<T> step, T context)
        {
            var stopwatch = new Stopwatch();
            var logger = _kernel.Resolve<ILogger>();
            logger.WriteBanner();
            logger.InfoFormat("Running step '{0}'...", stepName);
            stopwatch.Start();
            step.Execute(context);
            stopwatch.Stop();
            logger.InfoFormat("Step '{0}' completed.", stepName);
            logger.InfoFormat("Execution Time: ({0}).", stopwatch.Elapsed.ToString());
            logger.WriteBanner();

        }
    }
}
