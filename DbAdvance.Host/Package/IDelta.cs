using System.Collections.Generic;

namespace DbAdvance.Host.Package
{
    public interface IDelta
    {
        string Version { get; }

        IEnumerable<ScriptAccessor> CommitScripts { get; }

        IEnumerable<ScriptAccessor> RollbackScripts { get; }
    }
}