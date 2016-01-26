using System;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Pipeline;
using DbAdvance.Host.Usages.Up.Stages;

namespace DbAdvance.Host.Usages.Up
{
    /// <summary>
    /// Basic pipeline for applying forward-only changes to the target database via 
    /// a series of defined steps for a series of supplied scripts.
    /// </summary>
    public sealed class UpPipelineFactory :
        BasePipelineFactory<CommandPipelineContext>
    {
        public static readonly string UpPipelineFactoryCommand = "up";

        public override Type PipelineType
        {
            get { return typeof (UpPipeline); }
        }

        public UpPipelineFactory(IKernel kernel) : base(kernel)
        {
            Aliases = new string[] {UpPipelineFactoryCommand};
        }
        
    }
}