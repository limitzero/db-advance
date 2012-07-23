using System.Collections.Generic;

namespace DbAdvance.Host
{
    public class DatabaseScriptVersion
    {
        public string FromVersion { get; set; }

        public string ToVersion { get; set; }

        public IEnumerable<string> CommitScripts { get; set; }

        public IEnumerable<string> RollbackScripts { get; set; }
    }
}