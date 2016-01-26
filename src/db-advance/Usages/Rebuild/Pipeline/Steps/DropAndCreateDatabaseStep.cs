using System.Data.SqlClient;
using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Rebuild.Pipeline.Steps
{
    public class DropAndCreateDatabaseStep : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public DropAndCreateDatabaseStep(IKernel kernel, 
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            using (SqlConnection masterConnection = _configuration.GetConnectionToMaster())
            using (SqlCommand createDbCommand = masterConnection.CreateCommand())
            {
                var database = _configuration.GetDatabaseName(); 

                var statement = new StringBuilder();
                statement
                    .AppendFormat("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", database)
                    .AppendLine()
                    .AppendFormat("IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')",
                      database).AppendLine()
                    .AppendFormat("DROP DATABASE [{0}]", database).AppendLine()
                    .AppendLine()
                    .AppendFormat("CREATE DATABASE [{0}]", database).AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", database)
                    .AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", database).AppendLine();

                createDbCommand.CommandText = statement.ToString();
                createDbCommand.ExecuteNonQuery();
                masterConnection.Close();
            }
        }
    }
}