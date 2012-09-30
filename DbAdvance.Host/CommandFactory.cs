using System;
using System.Collections.Generic;

using DbAdvance.Host.Commands;

using Microsoft.Practices.Unity;

namespace DbAdvance.Host
{
    class CommandFactory
    {
        private readonly IUnityContainer container;

        public CommandFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public ICommand Create(IList<string> args)
        {
            if (args.Count < 3)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var configuration = container.Resolve<Configuration>();

            configuration.ConnectionString = args[1];
            configuration.DatabaseName = args[2];

            switch (args[0])
            {
                case "-setbaseversion":
                    return CreateSetBaseVersionCommand(args);

                case "-commit":
                    return CreateCommitCommand(args);

                case "-committoversion":
                    return CreateCommitToVersionCommand(args);

                case "-rollback":
                    return CreateRollbackCommand(args);

                case "-rollbacktoversion":
                    return CreateRollbackToVersionCommand(args);

                default:
                    return container.Resolve<PrintUsageCommand>();
            }
        }

        private ICommand CreateSetBaseVersionCommand(IList<string> args)
        {
            var version = ParseVersion(args, 3);

            var command = container.Resolve<SetBaseVersionCommand>();

            command.Version = version;

            return command;
        }

        private ICommand CreateCommitCommand(IList<string> args)
        {
            if (args.Count < 4)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var command = container.Resolve<CommitCommand>();

            command.PackagePath = args[3];

            return command;
        }

        private ICommand CreateRollbackCommand(IList<string> args)
        {
            if (args.Count < 4)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var command = container.Resolve<RollbackCommand>();

            command.PackagePath = args[3];

            return command;
        }

        private ICommand CreateCommitToVersionCommand(IList<string> args)
        {
            if (args.Count < 5)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var version = ParseVersion(args, 4);

            if (version == null)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var command = container.Resolve<CommitToVersionCommand>();

            command.PackagePath = args[3];
            command.Version = version;

            return command;
        }

        private ICommand CreateRollbackToVersionCommand(IList<string> args)
        {
            if (args.Count < 5)
            {
                return container.Resolve<PrintUsageCommand>();
            }

            var version = ParseVersion(args, 4);

            var command = container.Resolve<RollbackToVersionCommand>();

            command.PackagePath = args[3];
            command.Version = version;

            return command;
        }

        private static string ParseVersion(IList<string> args, int index)
        {
            if (args.Count < (index + 1))
            {
                return null;
            }

            if (!String.IsNullOrEmpty(args[index]) && Int32.Parse(args[index]) != 0)
            {
                return args[index];
            }

            return null;
        }
    }
}