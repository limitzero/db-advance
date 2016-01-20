using System.Linq;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.Commands.Steps.VersioningStrategy;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Steps
{
    public sealed class VersionAllScriptsForRunStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public VersionAllScriptsForRunStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            if (!context.AllScriptsRun.Any())
                return;

            DetermineNextVersion(context);
            UpdateAllExecutedScriptsToCurrentVersion(context);
        }

        private void DetermineNextVersion(CommandPipelineContext context)
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

        private void UpdateAllExecutedScriptsToCurrentVersion(
            CommandPipelineContext context)
        {
            context.AllScriptsRun
                .ForEach(script => UpdateScriptForVersion(context.ToVersion, script));

            context.AllScriptErrors
                .ForEach(script => UpdateScriptErrorForVersion(context.ToVersion, script));
        }

        private void UpdateScriptForVersion(string version, ScriptsRunInfo info)
        {
            var update = string.Format("update [{0}] set version = '{1}' where id = {2}",
                ScriptsRunInfo.GetTableName(),
                version,
                info.Id.ToString());

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(update);
            }
        }

        private void UpdateScriptErrorForVersion(string version, ScriptsRunErrorInfo info)
        {
            var update = string.Format("update [{0}] set version = '{1}' where id = {2}",
                ScriptsRunErrorInfo.GetTableName(),
                version,
                info.Id.ToString());

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(update);
            }
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