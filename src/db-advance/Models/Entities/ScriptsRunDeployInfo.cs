using System;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace DbAdvance.Host.Models.Entities
{
    [Table("ScriptsRunDeployInfo")]
    public class ScriptsRunDeployInfo
    {
        [Key]
        public long Id { get; set; }
        public long? VersionInfoId { get; set; }
        public string DeployPackageName { get; set; }
        public byte[] DeployPackageContent { get; set; }
        public DateTime? DeployedOn { get; set; }
        public string DeployedBy { get; set; }

        public static string GetTableName()
        {
            var attr = typeof (ScriptsRunDeployInfo)
                .GetCustomAttributes(typeof (TableAttribute), false)
                .First() as TableAttribute;
            return attr.Name;
        }
    }
}