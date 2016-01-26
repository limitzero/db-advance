using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.Commands;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages.Deploy.Pipeline.Steps
{
    public class DeployPackageStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDatabaseConnectorConfiguration _configuration;

        public DeployPackageStep(IKernel kernel,
            IFileSystem fileSystem,
            IDatabaseConnectorConfiguration configuration) : base(kernel)
        {
            _fileSystem = fileSystem;
            _configuration = configuration;
        }

        public override void Execute(CommandPipelineContext context)
        {
            DeployPackage(context);
            RecordScriptsDeployInfo(context);
        }

        private void DeployPackage(CommandPipelineContext context)
        {
            context.Options.ConfigureForUp();
            var connector = Kernel.Resolve<CommandPipelineFactoryConnector>();
            connector.Apply(context);
        }

        private void RecordScriptsDeployInfo(CommandPipelineContext context)
        {
            var deployedScripts = new HashSet<string>();
            _fileSystem.GetFilesInPath(deployedScripts, context.Options.ScriptsPath);

            var fileNames = deployedScripts
                .Select(script => Path.GetFileName(script))
                .Distinct()
                .ToList();

            VersionInfo versionInfo;
            var packageAsZipFile = GetPackageAsZipFileFromLatestDeploymentDirectory(context);
            var scripts = GetAllScriptsForCurrentVersion(out versionInfo);

            var deployed = (from file in fileNames
                from script in scripts
                where file == script.ScriptName
                select script).Distinct().ToList();

            if (!deployed.Any()) return;

            var info = CreateDeployInfo(context, versionInfo);

            deployed
                .ForEach(deploy => UpdateScriptInfoWithDeploymentId(info, deploy));

            Logger.InfoFormat("Labeled all scripts for deployment based on version {0} with latest deployment package {1}.", 
                versionInfo.Version, Path.GetFileName(packageAsZipFile));
        }

        private IEnumerable<ScriptsRunInfo> GetAllScriptsForCurrentVersion(out VersionInfo versionInfo)
        {
             versionInfo = GetMaxVersion();

            if (versionInfo == null) return new List<ScriptsRunInfo>();

            var statement = string.Format("select s.* from [{0}] s where [VersionInfoId] ='{1}'",
                ScriptsRunInfo.GetTableName(),
                versionInfo.Id);

            using (var connection = _configuration.GetConnection())
            {
                var scripts = connection.Query<ScriptsRunInfo>(statement).ToList();
                return scripts;
            }
        }

        private VersionInfo GetMaxVersion()
        {
            var statement = string.Format("select top 1 v.* from [{0}] v order by id desc", VersionInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                var version = connection.Query<VersionInfo>(statement).FirstOrDefault();
                return version;
            }
        }

        private ScriptsRunDeployInfo CreateDeployInfo(CommandPipelineContext context, VersionInfo versionInfo)
        {
            var packageAsZipFile = GetPackageAsZipFileFromLatestDeploymentDirectory(context);
            
            if (string.IsNullOrEmpty(packageAsZipFile)) return null;

            var packageName = Path.GetFileName(packageAsZipFile);
            var packageContent = File.ReadAllBytes(packageAsZipFile);

            var info = new ScriptsRunDeployInfo
            {
                VersionInfoId = versionInfo.Id,
                DeployPackageName = packageName,
                DeployPackageContent = packageContent,
                DeployedOn = DateTime.Now,
                DeployedBy = string.Format(@"{0}\{1}",
                    Environment.UserDomainName,
                    Environment.UserName)
            };

            using (var connection = _configuration.GetConnection())
            {
                var id = connection.Insert<ScriptsRunDeployInfo>(info);
                if (info.Id == 0)
                    info.Id = id;
            }

            if (!string.IsNullOrEmpty(packageAsZipFile))
                Logger.InfoFormat("Package '{0}' recorded as deployed.", Path.GetFileName(packageAsZipFile));

            return info;
        }

        private string GetPackageAsZipFileFromLatestDeploymentDirectory(CommandPipelineContext context)
        {
            var packageAsZipFile =
               Directory.GetFiles(context.Options.ScriptsPath, "*.zip")
                   .FirstOrDefault();
            return packageAsZipFile;
        }

        private void UpdateScriptInfoWithDeploymentId(ScriptsRunDeployInfo deployInfo, ScriptsRunInfo runInfo)
        {
            var statement = string.Format("update [{0}] set [DeployInfoId] = {1} where [Id] = {2}",
                ScriptsRunInfo.GetTableName(),
                deployInfo.Id.ToString(),
                runInfo.Id.ToString());

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(statement);
            }
        }
    }
}