using System;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Usages;

namespace DbAdvance.Host
{
    public sealed class DbAdvanceRunner : IDisposable
    {
        private IWindsorContainer _container;
        private bool _disposed;

        ~DbAdvanceRunner()
        {
            Dispose(true);
        }

        public void Run(string[] args)
        {
            InstantiateContainerAndRegisterComponents();

            var options = InitializeOptionsFromCommandLineArguments(args);

            var pipelineConnector = _container.Resolve<CommandPipelineFactoryConnector>();

            try
            {
                RunDesiredCommand(options, pipelineConnector);  
            }
            catch (Exception runnerException)
            {
                ExitRunnerWithFailure(options, runnerException);
            }

            ExitRunnerWithSuccess(options);
        }

        private void RunDesiredCommand(
            DbAdvancedOptions options,
            CommandPipelineFactoryConnector connector)
        {
            connector.Apply(options);
        }

        private DbAdvancedOptions InitializeOptionsFromCommandLineArguments(string[] args)
        {
            var options = new DbAdvancedOptions(_container.Resolve<ILogger>());
            options.Configure(args);

            _container.Register(Component
                .For<DbAdvancedOptions>()
                .Instance(options));

            return options;
        }

        private void InstantiateContainerAndRegisterComponents()
        {
            _container = new WindsorContainer();
            _container.Install(new DbAdvanceInstaller());

            var title = string.Format("db-advance : Database migration tool for schema changes (v.{0})",
                this.GetType().Assembly.GetName().Version);
            Console.Title = title;

            var environment = string.Format("db-advance console runner ({0}-bit .NET {1})", IntPtr.Size*8,
                Environment.Version);
            _container.Resolve<ILogger>().Info(environment);
        }

        private void ExitRunnerWithSuccess(DbAdvancedOptions options)
        {
            InspectForInteractiveSession(options);
            Environment.Exit(0);
        }

        private void ExitRunnerWithFailure(DbAdvancedOptions options,
            Exception exception = null)
        {
            if (exception != null)
            {
                _container.Resolve<ILogger>().Error(exception.Message, exception);
            }

            InspectForInteractiveSession(options);
            Environment.Exit(-1);
        }

        private void InspectForInteractiveSession(DbAdvancedOptions options)
        {
            if (options.Wait)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_container != null)
                {
                    _container.Dispose();
                }
                _container = null;
            }

            _disposed = true;
        }
    }
}