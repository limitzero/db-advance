using System.Collections.Generic;

namespace DbAdvance.Host.Pipeline
{
    /// <summary>
    ///  Marker interface for a pipeline
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        /// Gets or sets the indicator to stop processing subsequent steps in the pipeline:
        /// </summary>
        bool Halt { get; set; }
    }

    public interface IPipeline<T> : IPipeline where T : BasePipelineContext
    {
        IEnumerable<IPipelineStep<T>> PreProcessingSteps { get; }
        IEnumerable<IPipelineStep<T>> ProcessingSteps { get; }
        IEnumerable<IPipelineStep<T>> PostProcessingSteps { get; }

        IPipelineStep<T> ResolveStep<S>()
            where S : class, IPipelineStep<T>;

        void Configure();
        void Execute(T context);
    }
}