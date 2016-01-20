namespace DbAdvance.Host.Pipeline
{
    /// <summary>
    ///  Marker interface for a pipeline step
    /// </summary>
    public interface IPipelineStep
    {
        bool HaltPipeline { get; set; }
    }

    public interface IPipelineStep<T> : IPipelineStep
        where T : BasePipelineContext
    {
        IPipeline<T> Pipeline { get; set; }
        void Execute(T context);
    }
}