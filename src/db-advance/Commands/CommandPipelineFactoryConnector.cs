using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands
{
    public class CommandPipelineFactoryConnector
    {
        private readonly IKernel _kernel;

        public CommandPipelineFactoryConnector(IKernel kernel)
        {
            _kernel = kernel;
        }

        public CommandPipelineContext Apply(DbAdvanceCommandLineOptions options)
        {
            return Apply(options.Command, options);
        }

        public CommandPipelineContext Apply(string command,
            DbAdvanceCommandLineOptions options = null)
        {
            var pipelineFactory = _kernel
                .ResolveAll<IPipelineFactory<CommandPipelineContext>>()
                .FirstOrDefault(f => f.Aliases.Contains(command));

            if (pipelineFactory == null) return null;

            var context = new CommandPipelineContext
            {
                Options = options ?? _kernel.Resolve<DbAdvanceCommandLineOptions>()
            };

            var pipeline = pipelineFactory.Create();

            if (pipeline == null) return null;
            pipelineFactory.Execute(pipeline, context);

            return context;
        }

        public void Apply(CommandPipelineContext context)
        {
            var pipelineFactory = _kernel
                .ResolveAll<IPipelineFactory<CommandPipelineContext>>()
                .FirstOrDefault(f => f.Aliases.Contains(context.Options.Command));

            if (pipelineFactory == null) return;
            var pipeline = pipelineFactory.Create();

            if (pipeline == null) return;
            pipelineFactory.Execute(pipeline, context);
        }
    }
}