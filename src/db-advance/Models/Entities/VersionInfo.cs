using System;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace DbAdvance.Host.Models.Entities
{
    [Table("VersionInfo")]
    public class VersionInfo
    {
        public long Id { get; set; }
        public string Version { get; set; }
        public DateTime? EntryDate { get; set; }
        public string EnteredBy { get; set; }

        public static string GetTableName()
        {
            var attr = typeof (VersionInfo)
                .GetCustomAttributes(typeof (TableAttribute), false)
                .First() as TableAttribute;
            return attr.Name;
        }
    }
}