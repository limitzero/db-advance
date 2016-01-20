using Castle.MicroKernel;

namespace DbAdvance.Host.Commands.Rollback.Pipeline
{
    public class RollbackPipeline
        : BaseMutateTargetDatabasePipeline
    {
        public RollbackPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<RunDownStep>());

            base.Configure();
        }
    }
}