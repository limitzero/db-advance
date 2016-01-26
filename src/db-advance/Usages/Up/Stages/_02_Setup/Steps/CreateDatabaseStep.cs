using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._02_Setup.Steps
{
    public class CreateDatabaseStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DatabaseConnectorFactory _factory;

        public CreateDatabaseStep(IKernel kernel, 
            IDatabaseConnectorConfiguration configuration, 
            DatabaseConnectorFactory factory) : base(kernel)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
           if(!IsDatabasePresent())
                CreateDatabase();
        }

        private bool IsDatabasePresent()
        {
            base.Logger.InfoFormat("Creating {0} on instance {1} if it does not exist..", 
                _configuration.GetDatabaseName(), 
                _configuration.GetDatabaseServerName());

            var connector = _factory.UseBasicConnector();
            return connector.DatabaseExists();
        }

        private void CreateDatabase()
        {
            using (var master = _configuration.GetConnectionToMaster())
            using (var command = master.CreateCommand())
            {
                var statement = new StringBuilder();
                statement
                    .AppendFormat("IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')",
                        _configuration.GetDatabaseName()).AppendLine()
                    .AppendFormat("CREATE DATABASE [{0}]", _configuration.GetDatabaseName()).AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", _configuration.GetDatabaseName())
                    .AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", _configuration.GetDatabaseName()).AppendLine();

                command.CommandText = statement.ToString();
                command.ExecuteNonQuery();
            }
        }
    }
}