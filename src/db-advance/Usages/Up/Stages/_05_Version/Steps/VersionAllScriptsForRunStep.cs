using System.Linq;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Dapper;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Commands.Steps.VersioningStrategy;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Up.Stages._05_Version.Steps
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
            long? version = null;
            var versionInfo = GetVersionInfoForCurrentVersion(context.ToVersion);

            if (versionInfo != null)
                version = versionInfo.Id;
            
            context.AllScriptsRun
                .ForEach(script => UpdateScriptForVersion(script, version));

            context.AllScriptErrors
                .ForEach(script => UpdateScriptErrorForVersion(script, version));
        }

        private void UpdateScriptForVersion(ScriptsRunInfo info, long? versionId = null)
        {
            if (!versionId.HasValue) return;

            var update = string.Format("update [{0}] set [versioninfoid] = {1} where id = {2}",
                ScriptsRunInfo.GetTableName(),
                versionId,
                info.Id.ToString());

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(update);
            }
        }

        private void UpdateScriptErrorForVersion(ScriptsRunErrorInfo info, long? versionId = null)
        {
            if (!versionId.HasValue) return;

            var update = string.Format("update [{0}] set [versioninfoid] = {1} where id = {2}",
                ScriptsRunErrorInfo.GetTableName(),
                versionId,
                info.Id.ToString());

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(update);
            }
        }

        private VersionInfo GetVersionInfoForCurrentVersion(string version)
        {
            var statement = string.Format("select top 1 v.* from [{0}] v where [version] = '{1}' order by id desc",
                VersionInfo.GetTableName(), 
                version);

            using (var connection = _configuration.GetConnection())
            {
                return connection.Query<VersionInfo>(statement).FirstOrDefault();
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