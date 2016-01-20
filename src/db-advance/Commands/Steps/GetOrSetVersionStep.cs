using System.Linq;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.Commands.Steps.VersioningStrategy;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Steps
{
    public sealed class GetOrSetVersionStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public GetOrSetVersionStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            Logger.WriteBanner();

            Logger.Info("STAGE: Determine migration version information");

            DetermineMigrationVersionInformation(context);

            if ((context.ToVersion == context.FromVersion))
            {
                Logger.InfoFormat("Supplied migration version'{0}' same as current version. Aborting..",
                    context.ToVersion,
                    context.FromVersion);
                Pipeline.Halt = true;
            }

            if (!string.IsNullOrEmpty(context.Options.Environment))
            {
                Logger.InfoFormat("'{0}' tag found for isolating environment specific scripts...",
                    context.Options.Environment);
            }

            Logger.WriteBanner();
        }

        private void DetermineMigrationVersionInformation(CommandPipelineContext context)
        {
            var databaseVersion = GetDatabaseVersion();
            var desiredVersion = context.Options.Version;

            var versioningStrategy = Kernel
                .ResolveAll<BaseVersionDatabaseSpecification>()
                .FirstOrDefault(s => s.IsMatch(databaseVersion, desiredVersion));

            if (versioningStrategy == null)
                return; // need to figure this out...it should not happen!!!

            versioningStrategy.Execute(databaseVersion, desiredVersion);

            context.FromVersion = versioningStrategy.FromVersion;
            context.ToVersion = versioningStrategy.ToVersion;
        }

        private string GetDatabaseVersion()
        {
            var statement = string.Format("select top 1 v.* from [{0}] v order by id desc",
                VersionInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                var max = connection.Query<VersionInfo>(statement).FirstOrDefault();
                if (max == null)
                    return string.Empty;
                else
                    return max.Version;
            }
        }
    }
}