using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Clean.Pipeline.Steps
{
    public sealed class CleanDatabaseStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public CleanDatabaseStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.InfoFormat("Creating/re-creating database '{0}' on instance '{1}'...",
                _configuration.GetDatabaseName(),
                _configuration.GetDatabaseServerName());

            CleanTargetDatabaseViaDropAndCreate(context);

            Logger.InfoFormat("Database '{0}' on instance '{1}' created.",
                _configuration.GetDatabaseName(),
                _configuration.GetDatabaseServerName());
        }

        private void CleanTargetDatabaseViaDropAndCreate(CommandPipelineContext context)
        {
            using (var master = _configuration.GetConnectionToMaster())
            using (var rebuildCommand = master.CreateCommand())
            {
                var statement = new StringBuilder();
                statement
                    .AppendFormat("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", _configuration.GetDatabaseName())
                    .AppendLine()
                    .AppendFormat("IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')",
                        _configuration.GetDatabaseName()).AppendLine()
                    .AppendFormat("DROP DATABASE [{0}]", _configuration.GetDatabaseName()).AppendLine()
                    .AppendLine()
                    .AppendFormat("CREATE DATABASE [{0}]", _configuration.GetDatabaseName()).AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", _configuration.GetDatabaseName())
                    .AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", _configuration.GetDatabaseName()).AppendLine();

                rebuildCommand.CommandText = statement.ToString();
                rebuildCommand.ExecuteNonQuery();
                master.Close();
            }
        }
    }
}