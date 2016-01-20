using System.Collections.Generic;

namespace DbAdvance.Host.Package
{
    public class Delta : IDelta
    {
        public string Tag { get; set; }
        public string ScriptFolder { get; set; }
        public IEnumerable<ScriptAccessor> Scripts { get; set; }

        public IEnumerable<ScriptAccessor> CommitScripts { get; set; }

        public IEnumerable<ScriptAccessor> RollbackScripts { get; set; }
    }
}