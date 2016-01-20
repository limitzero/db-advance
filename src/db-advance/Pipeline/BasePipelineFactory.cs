using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;

namespace DbAdvance.Host.Pipeline
{
    public abstract class BasePipelineFactory<T> : IPipelineFactory<T>
        where T : BasePipelineContext
    {
        private readonly IKernel _kernel;
        public IEnumerable<string> Aliases { get; protected set; }
        public abstract Type PipelineType { get; }

        protected BasePipelineFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IPipeline Create()
        {
            var pipeline = _kernel.ResolveAll<IPipeline>()
                .FirstOrDefault(p => p.GetType() == PipelineType);
            return pipeline;
        }

        public void Execute(IPipeline pipeline, BasePipelineContext context)
        {
            var executable_pipeline = pipeline as IPipeline<T>;
            var executable_context = context as T;

            if (executable_pipeline == null)
                throw new InvalidOperationException("Bad pipeline...");

            if (executable_context == null)
                throw new InvalidOperationException("Bad context...");

            executable_pipeline.Configure();
            executable_pipeline.Execute(executable_context);
        }
    }
}