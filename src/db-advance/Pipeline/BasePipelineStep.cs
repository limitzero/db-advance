using Castle.Core.Logging;
using Castle.MicroKernel;

namespace DbAdvance.Host.Pipeline
{
    public abstract class BasePipelineStep<T> : IPipelineStep<T>
        where T : BasePipelineContext
    {
        public IKernel Kernel { get; private set; }

        public IPipeline<T> Pipeline { get; set; }

        public bool HaltPipeline { get; set; }

        public ILogger Logger { get; set; }

        protected BasePipelineStep(IKernel kernel)
        {
            Kernel = kernel;
        }

        public abstract void Execute(T context);
    }
}