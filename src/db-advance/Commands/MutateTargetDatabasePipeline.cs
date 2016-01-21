using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands.Steps;
using DbAdvance.Host.Package.ChangeDetection;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands
{
    public abstract class BaseMutateTargetDatabasePipeline
        : BasePipeline<CommandPipelineContext>
    {
        protected BaseMutateTargetDatabasePipeline(IKernel kernel) : base(kernel)
        {
        }

        public override void Configure()
        {
            RecordPreProcessingSteps(
                ResolveStep<ReportPreUpgradeInformationStep>(),
                ResolveStep<RunBeforeAllStep>(),
                ResolveStep<RunOneTimeStep>());

            // here is where the folder specific procesing steps will be placed 
            // per each individual pipeline for the associated up or down command:

            RecordPostProcessingSteps(
                ResolveStep<RunAfterAllStep>(),
                ResolveStep<VersionAllScriptsForRunStep>(),
                ResolveStep<ReportPostUpgradeInformationStep>());
        }
    }

    public abstract class BaseUseScriptsInFolderForMutationStep :
        BasePipelineStep<CommandPipelineContext>
    {
        public abstract string Folder { get; }

        protected BaseUseScriptsInFolderForMutationStep(IKernel kernel) : base(kernel)
        {
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.InfoFormat("Examining all scripts in folder './{0}'...", Folder);

            var scriptFolder = Kernel.ResolveAll<BaseScriptFolder>()
              .FirstOrDefault(sf => sf.Folder == Folder);

            if (scriptFolder == null) return;

            var scripts = scriptFolder.Examine();
            if (!scripts.Any()) return;

            Logger.InfoFormat("Running all scripts in folder './{0}'...", Folder);

            context.FolderDeltas = scriptFolder.CreateDeltasFromScripts(scripts);

            var applyScriptsStep = Pipeline.ResolveStep<ApplyScriptsStep>() as ApplyScriptsStep;
            applyScriptsStep.OnScriptInfoRecorded += (info) => context.RecordScriptInfoRun(info);
            applyScriptsStep.OnScriptInfoErrorRecorded += (info) => context.RecordScriptInfoRunError(info);

            InspectApplyScriptsStepBeforeExecution(applyScriptsStep);
            applyScriptsStep.Execute(context);

            Logger.InfoFormat("Scripts in folder './{0}' executed.", Folder);
        }

        public virtual void InspectApplyScriptsStepBeforeExecution(ApplyScriptsStep applyScriptsStep)
        {
         
        }
    }

    public sealed class RunBeforeAllStep
        : BaseUseScriptsInFolderForMutationStep
    {
        public override string Folder
        {
            get { return FolderStructure.RunBeforeAll; }
        }

        public RunBeforeAllStep(IKernel kernel) : base(kernel)
        {
        }
    }

    public sealed class RunOneTimeStep
        : BaseUseScriptsInFolderForMutationStep
    {
        public override string Folder { get { return FolderStructure.RunOneTime; } }

        public RunOneTimeStep(IKernel kernel) : base(kernel)
        {
        }
    }

    public sealed class RunUpStep
        : BaseUseScriptsInFolderForMutationStep
    {
        public override string Folder { get { return FolderStructure.Up; } }

        public RunUpStep(IKernel kernel) : base(kernel)
        {
        }

        public override void InspectApplyScriptsStepBeforeExecution(ApplyScriptsStep applyScriptsStep)
        {
        }
    }

    public sealed class RunDownStep
        : BaseUseScriptsInFolderForMutationStep
    {
        public override string Folder { get { return FolderStructure.Down; } }

        public RunDownStep(IKernel kernel) : base(kernel)
        {
        }

        public override void InspectApplyScriptsStepBeforeExecution(ApplyScriptsStep applyScriptsStep)
        {
        }
    }

    public sealed class RunAfterAllStep
        : BaseUseScriptsInFolderForMutationStep
    {
        public override string Folder { get { return FolderStructure.RunAfterAll; } }

        public RunAfterAllStep(IKernel kernel) : base(kernel)
        {
        }
    }
}