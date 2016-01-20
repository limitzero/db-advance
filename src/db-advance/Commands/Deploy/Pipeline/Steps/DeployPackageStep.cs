using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel;
using Dapper;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Deploy.Pipeline.Steps
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
            _fileSystem.GetFilesInPath(deployedScripts, context.Options.Path);

            var fileNames = deployedScripts
                .Select(script => Path.GetFileName(script))
                .Distinct()
                .ToList();

            var scripts = GetAllScriptsForCurrentVersion();

            var deployed = (from file in fileNames
                from script in scripts
                where file == script.ScriptName
                select script).Distinct().ToList();

            if (!deployed.Any()) return;

            var info = CreateDeployInfo(context);

            deployed
                .ForEach(deploy => UpdateScriptInfoWithDeploymentId(info, deploy));
        }


        private IEnumerable<ScriptsRunInfo> GetAllScriptsForCurrentVersion()
        {
            var version = GetMaxVersion();

            if (version == null) return new List<ScriptsRunInfo>();

            var statement = string.Format("select s.* from [{0}] s where [version] ='{1}'",
                ScriptsRunInfo.GetTableName(),
                version.Version);

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

        private ScriptsRunDeployInfo CreateDeployInfo(CommandPipelineContext context)
        {
            var zip =
                Directory.GetFiles(context.Options.Path, "*.zip")
                    .FirstOrDefault();

            if (string.IsNullOrEmpty(zip)) return null;

            var packageName = Path.GetFileName(zip);
            var packageContent = File.ReadAllBytes(zip);

            var info = new ScriptsRunDeployInfo
            {
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

            return info;
        }

        private string GetZipFileName(CommandPipelineContext context)
        {
            string zipFileName;
            if (File.Exists(context.Options.PackageName))
                zipFileName = Path.GetFileNameWithoutExtension(context.Options.PackageName);
            else
            {
                zipFileName = context.Options.PackageName.Replace(".zip", string.Empty);
            }

            return zipFileName;
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