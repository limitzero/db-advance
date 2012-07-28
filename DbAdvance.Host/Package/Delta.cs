using System.Collections.Generic;

namespace DbAdvance.Host.Package
{
    public class Delta : IDelta
    {
        public string Version { get; set; }

        public IEnumerable<ScriptAccessor> CommitScripts { get; set; }

        public IEnumerable<ScriptAccessor> RollbackScripts { get; set; }
    }
}
