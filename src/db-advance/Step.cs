using System.Collections.Generic;
using DbAdvance.Host.Package;

namespace DbAdvance.Host
{
    public class Step
    {
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }

        public IEnumerable<ScriptAccessor> Scripts { get; set; }
    }
}