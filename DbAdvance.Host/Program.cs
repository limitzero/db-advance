using System;
using System.Collections.Generic;

using DbAdvance.Host.Archiver;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new Logger();

            args = new[]
                {
                    "-c",
                    "Data Source=ci.tfn.com;User Id=sa; Password=Password123;",
                    "BetaIntegration2",
                    @"d:\rel\build.zip"
                };

            if (CheckParameters(args, logger, 3)) return;

            var command = args[0];
            var connectionString = args[1];
            var databaseName = args[2];

            var engine = GetEngine(logger, connectionString, databaseName);

            engine.ReportVersions();
            
            switch (command)
            {
                case "-sbv":
                    if (CheckParameters(args, logger, 4)) throw new Exception();

                    try
                    {
                        engine.SetBaseDatabaseVersion(args[3]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }

                    break;

                case "-c":
                    if (CheckParameters(args, logger, 4)) throw new Exception();

                    try
                    {
                        engine.Commit(args[3]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }
                    
                    break;
                        
                case "-cv":
                    if (CheckParameters(args, logger, 5)) throw new Exception();

                    try
                    {
                        engine.CommitVersion(args[3], args[4]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }

                    break;

                case "-r":
                    if (CheckParameters(args, logger, 4)) throw new Exception();

                    try
                    {
                        engine.Rollback(args[3]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }

                    break;


                case "-rv":
                    if (CheckParameters(args, logger, 4)) throw new Exception();

                    try
                    {
                        engine.RollbackVersion(args[3], args[4]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }

                    break;

                case "-rc":
                    if (CheckParameters(args, logger, 5)) throw new Exception();

                    try
                    {
                        engine.RollbackAndCommit(args[3], args[4]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(e.Message);
                        throw;
                    }

                    break;

                default:
                    logger.Log("Unknown command: {0}", command);
                    PrintUsage(logger);
                    throw new Exception();
            }

        }

        private static bool CheckParameters(ICollection<string> args, ILogger logger, int number)
        {
            if (args.Count < number)
            {
                PrintUsage(logger);

                return true;
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
            logger.Log("Db Advance. Incorrect Parameters.");
        }
    }
}
