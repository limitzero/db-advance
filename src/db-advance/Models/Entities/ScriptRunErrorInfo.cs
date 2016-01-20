using System;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace DbAdvance.Host.Models.Entities
{
    [Table("ScriptsRunErrorInfo")]
    public class ScriptsRunErrorInfo
    {
        [Key]
        public long Id { get; set; }

        public string Version { get; set; }
        public string ScriptName { get; set; }
        public string ScriptText { get; set; }
        public string ScriptError { get; set; }
        public DateTime? EntryDate { get; set; }

        public static string GetTableName()
        {
            var attr = typeof (ScriptsRunErrorInfo)
                .GetCustomAttributes(typeof (TableAttribute), false)
                .First() as TableAttribute;
            return attr.Name;
        }
    }
}