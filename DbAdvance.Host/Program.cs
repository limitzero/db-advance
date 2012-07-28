using System.Collections.Generic;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new Logger();

            if (CheckParameters(args, logger, 3)) return;

            var command = args[0];
            var connectionString = args[1];
            var databaseName = args[2];

            var engine = GetEngine(logger, connectionString, databaseName);

            
            switch (command)
            {
                case "-c":
                    if (CheckParameters(args, logger, 4)) return;

                    engine.ReportVersions();
                    engine.Commit(args[3]);

                    break;

                case "-r":
                    if (CheckParameters(args, logger, 4)) return;

                    engine.ReportVersions();
                    engine.Rollback(args[3]);

                    break;

                case "-rc":
                    if (CheckParameters(args, logger, 5)) return;

                    engine.ReportVersions();
                    engine.RollbackAndCommit(args[3], args[4]);

                    break;

                default:
                    logger.Log("Unknown command: {0}", command);
                    PrintUsage(logger);
                    break;
            }

        }

        private static bool CheckParameters(ICollection<string> args, ILogger logger, int number)
        {
            if (args.Count < number)
            {
                PrintUsage(logger);
            }

            return false;
        }

        private static Engine GetEngine(ILogger logger, string connectionString, string databaseName)
        {
            return new Engine(
                logger,
                new ZipArchiver(),
                new FileSystem(logger),
                new DatabaseConnectorFactory(logger, new Configuration() { ConnectionString = connectionString, DatabaseName = databaseName }),
                new PackageReader());
        }

        private static void PrintUsage(ILogger logger)
        {
            logger.Log("Db Advance:");
            logger.Log("assembly and class name.");
        }
    }
}
