using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Drop.Pipeline.Steps
{
    public class DropDatabaseStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DatabaseConnectorFactory _factory;

        public DropDatabaseStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration,
            DatabaseConnectorFactory factory) : base(kernel)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
            if (!IsDatabasePresent())
                return;

            DropDatabase();
        }

        private bool IsDatabasePresent()
        {
            var result = false;

            Logger.InfoFormat("Dropping {0} on instance {1} if it exists..",
                _configuration.GetDatabaseName(),
                _configuration.GetDatabaseServerName());

            try
            {
                var connector = _factory.UseBasicConnector();
                result = connector.DatabaseExists();
            }
            catch 
            {
            }

            return result;
        }

        private void DropDatabase()
        {
            using (var master = _configuration.GetConnectionToMaster())
            using (var command = master.CreateCommand())
            {
                var statement = new StringBuilder();
                statement
                    .AppendFormat("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                        _configuration.GetDatabaseName())
                    .AppendLine()
                    .AppendFormat("IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')",
                        _configuration.GetDatabaseName()).AppendLine()
                    .AppendFormat("DROP DATABASE [{0}]", _configuration.GetDatabaseName()).AppendLine();

                command.CommandText = statement.ToString();
                command.ExecuteNonQuery();
            }
        }
    }
}