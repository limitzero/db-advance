using System.Collections.Generic;

namespace DbAdvance.Host.ScriptScanner
{
    public interface IDatabaseScriptsScanner
    {
        List<DatabaseScriptVersion> GetScripts(string packageRootPath);
    }
}