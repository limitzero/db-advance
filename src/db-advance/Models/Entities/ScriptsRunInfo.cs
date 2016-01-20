using System;
using System.Linq;
using System.Text;
using Dapper.Contrib.Extensions;

namespace DbAdvance.Host.Models.Entities
{
    [Table("ScriptsRunInfo")]
    public class ScriptsRunInfo
    {
        private string _scriptText;

        [Key]
        public long Id { get; set; }
        public string Version { get; set; }
        public string Tag { get; set; }
        public bool IsRollbackScript { get; set; }
        public long? DeployInfoId { get; set; }
        public string ScriptName { get; set; }

        public string ScriptText
        {
            get { return _scriptText; }
            set
            {
                _scriptText = value;
                ComputeHash();
            }
        }

        public byte[] ScriptHash { get; set; }
        public DateTime? EntryDate { get; set; }

        public static string GetTableName()
        {
            var attr = typeof (ScriptsRunInfo)
                .GetCustomAttributes(typeof (TableAttribute), false)
                .First() as TableAttribute;
            return attr.Name;
        }

        private void ComputeHash()
        {
            if (!string.IsNullOrEmpty(ScriptText))
                ScriptHash = Encoding.ASCII.GetBytes(ScriptText);
        }
    }
}