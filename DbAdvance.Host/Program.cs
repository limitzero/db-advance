using System;

using DbAdvance.Host.DbConnectors;

using Microsoft.Practices.Unity;

namespace DbAdvance.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var container = new UnityContainer())
            {
                container.RegisterType<IDatabaseConnectorConfiguration, Configuration>();
                container.RegisterType<IFileSystem, FileSystem>();
                container.RegisterType<Configuration>(new ContainerControlledLifetimeManager());
                container.RegisterType<ILogger, Logger>();

                var command = new CommandFactory(container).Create(args);
                var logger = container.Resolve<Logger>();

                try
                {
                    command.Execute();
                }
                catch (Exception e)
                {
                    logger.Log(e.Message);
                    throw;
                }
            }
        }
    }
}