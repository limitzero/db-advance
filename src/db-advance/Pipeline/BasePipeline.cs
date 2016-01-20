using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;

namespace DbAdvance.Host.Pipeline
{
    public abstract class BasePipeline<T> : IPipeline<T>
        where T : BasePipelineContext
    {
        private readonly IKernel _kernel;
        public bool Halt { get; set; }
        public IEnumerable<IPipelineStep<T>> PreProcessingSteps { get; private set; }
        public IEnumerable<IPipelineStep<T>> ProcessingSteps { get; private set; }
        public IEnumerable<IPipelineStep<T>> PostProcessingSteps { get; private set; }

        protected BasePipeline(IKernel kernel)
        {
            _kernel = kernel;
        }

        public virtual void Configure()
        {
            IEnumerable<IPipelineStep<T>> noop = new List<IPipelineStep<T>>();
            PreProcessingSteps = noop;
            ProcessingSteps = noop;
            PostProcessingSteps = noop;
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

        public void Execute(T context)
        {
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

            foreach (var step in steps)
            {
                if (step == null) continue;

                try
                {
                    PreStepExecution(step, context);

                    if (Halt)
                        break;

                    step.Pipeline = this;
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
    }
}