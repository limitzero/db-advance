using System.Collections.Generic;

namespace DbAdvance.Host.Package
{
    public interface IDelta
    {
        string Tag { get; }

        string ScriptFolder { get; set; }

        IEnumerable<ScriptAccessor> Scripts { get; }

        IEnumerable<ScriptAccessor> CommitScripts { get; }

        IEnumerable<ScriptAccessor> RollbackScripts { get; }
    }
}