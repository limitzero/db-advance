using System;
using System.Collections.Generic;

namespace DbAdvance.Host.Pipeline
{
    public interface IPipelineFactory
    {
        Type PipelineType { get; }

        IEnumerable<string> Aliases { get; }

        IPipeline Create();
        void Execute(IPipeline pipeline, BasePipelineContext context);
    }

    public interface IPipelineFactory<T> : IPipelineFactory
        where T : BasePipelineContext
    {
    }
}