using System;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DbAdvance.Host.Archiver;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Commands.Steps.FolderRunStrategy;
using DbAdvance.Host.Commands.Steps.VersioningStrategy;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models;
using DbAdvance.Host.Package;
using DbAdvance.Host.Package.ChangeDetection;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host
{
    public sealed class DbAdvanceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            try
            {
                container.AddFacility<LoggingFacility>(facility =>
                    facility.LogUsing(LoggerImplementation.NLog)
                        .WithAppConfig());

                container.Resolve<ILogger>();
            }
            catch
            {
                container.Register(Component.For<ILogger>()
                    .ImplementedBy<ConsoleLogger>()
                    .LifestyleTransient());
            }

            container.Register(Component
                .For<IDatabaseConnectorConfiguration>()
                .ImplementedBy<DatabaseConnectorConfiguration>()
                .LifestyleSingleton());

            container.Register(Component
                .For<IFileSystem>()
                .ImplementedBy<FileSystem>()
                .LifestyleTransient());

            container.Register(Component
                .For<DatabaseConnectorFactory>()
                .LifestyleTransient());

            container.Register(Component
                .For<ZipArchiver>()
                .LifestyleTransient());

            container.Register(Component
                .For<PackageReader>()
                .LifestyleTransient());

            container.Register(Component
                .For<CommandPipelineFactoryConnector>()
                .LifestyleTransient());

            container.Register(Component
                .For<QueryObjectExecutor>()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn<BaseRunScriptsForFolderSpecification>()
                .WithServiceBase()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn<BaseVersionDatabaseSpecification>()
                .WithServiceBase()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn<BaseScriptFolder>()
                .WithServiceBase()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn<IQuery>()
                .WithServiceSelf()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn(typeof (IPipelineFactory<>))
                .WithServiceAllInterfaces()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn(typeof (IPipeline<>))
                .WithServiceAllInterfaces()
                .LifestyleTransient());

            container.Register(Classes.FromThisAssembly()
                .BasedOn(typeof (IPipelineStep<>))
                .WithServiceAllInterfaces()
                .LifestyleTransient());
        }
    }
}