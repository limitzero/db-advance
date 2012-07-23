using System.Collections.Generic;

namespace DbAdvance.Host.ScriptScanner
{
    public interface IDatabaseScriptsScanner
    {
        IEnumerable<DatabaseScriptVersion> GetScripts(string packageRootPath);
    }
}