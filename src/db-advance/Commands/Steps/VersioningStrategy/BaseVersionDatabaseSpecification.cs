using System;
using System.Linq;
using Castle.Core.Logging;
using Dapper;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Models.Entities;

namespace DbAdvance.Host.Commands.Steps.VersioningStrategy
{
    public abstract class BaseVersionDatabaseSpecification
        : IVersionDatabaseSpecification
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public ILogger Logger { get; set; }
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }

        protected BaseVersionDatabaseSpecification(
            IDatabaseConnectorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public abstract bool IsMatch(string currentVersion, string desiredVersion);

        public void Execute(string currentVersion, string desiredVersion)
        {
            ExecuteVersioningStrategy(currentVersion, desiredVersion);

            if (FromVersion == ToVersion)
            {
                Logger.InfoFormat("Database '{0}' is currently at version '{1}'.",
                    _configuration.GetDatabaseName(),
                    currentVersion);
            }
            else
            {
                Logger.InfoFormat("Attempting to upgrade database '{0}' from version '{1}' to '{2}'...",
                    _configuration.GetDatabaseName(),
                    FromVersion,
                    ToVersion);
            }
        }

        public abstract void ExecuteVersioningStrategy(string currentVersion, string desiredVersion);

        protected VersionInfo CreateNewVersion(VersionInfo versionInfo = null)
        {
            if (versionInfo == null)
                versionInfo = new VersionInfo();

            versionInfo.EnteredBy = string.Format(@"{0}\{1}",
                Environment.UserDomainName,
                Environment.UserName);

            versionInfo.EntryDate = DateTime.Now;

            using (var connection = _configuration.GetConnection())
            {
                var id = connection.Insert(versionInfo);
                if (versionInfo.Id == 0)
                    versionInfo.Id = id;
            }

            return versionInfo;
        }

        protected VersionInfo GetMaxVersionInfoById()
        {
            var statement = string.Format("select top 1 v.* from [{0}] v order by id desc",
                VersionInfo.GetTableName());

            using (var connection = _configuration.GetConnection())
            {
                var max = connection.Query<VersionInfo>(statement).FirstOrDefault();
                return max;
            }
        }

        protected VersionInfo GetVersionInfoByVersion(string version)
        {
            var statement = string.Format("select top 1 v.* from [{0}] v where [version] = '{1}'",
                VersionInfo.GetTableName(),
                version);

            using (var connection = _configuration.GetConnection())
            {
                var result = connection.Query<VersionInfo>(statement).FirstOrDefault();
                return result;
            }
        }

        protected void UpdateVersionInfo(VersionInfo versionInfo)
        {
            var statement = string.Format("update [{0}] set version = '{1}' where id = {2} ",
                VersionInfo.GetTableName(),
                versionInfo.Version,
                versionInfo.Id);

            using (var connection = _configuration.GetConnection())
            {
                connection.Execute(statement);
            }
        }
    }
}