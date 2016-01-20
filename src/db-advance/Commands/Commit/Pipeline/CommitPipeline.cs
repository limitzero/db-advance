using Castle.MicroKernel;

namespace DbAdvance.Host.Commands.Commit.Pipeline
{
    public sealed class CommitPipeline
        : BaseMutateTargetDatabasePipeline
    {
        public CommitPipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordProcessingSteps(
                ResolveStep<RunUpStep>());

            base.Configure();
        }
    }
}