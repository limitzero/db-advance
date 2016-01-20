using System.Data.SqlClient;
using System.Text;
using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Rebuild.Pipeline.Steps
{
    public class RecreateDatabaseStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public RecreateDatabaseStep(IKernel kernel, IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            var target = new SqlConnectionStringBuilder(_configuration.ConnectionString);

            Logger.InfoFormat("Dropping and re-creating database '{0}' on instance '{1}'...",
                target.InitialCatalog,
                target.DataSource);

            using (var master = _configuration.GetConnectionToMaster())
            using (var rebuildCommand = master.CreateCommand())
            {
                var statement = new StringBuilder();
                statement
                    .AppendFormat("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", target.InitialCatalog)
                    .AppendLine()
                    .AppendFormat("IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')",
                        target.InitialCatalog).AppendLine()
                    .AppendFormat("DROP DATABASE [{0}]", target.InitialCatalog).AppendLine()
                    .AppendLine()
                    .AppendFormat("CREATE DATABASE [{0}]", target.InitialCatalog).AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", target.InitialCatalog)
                    .AppendLine()
                    .AppendFormat("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", target.InitialCatalog).AppendLine();

                rebuildCommand.CommandText = statement.ToString();
                rebuildCommand.ExecuteNonQuery();
                master.Close();
            }

            Logger.InfoFormat("Database '{0}' on instance '{1}' dropped and recreated.",
                target.InitialCatalog,
                target.DataSource);
        }
    }
}