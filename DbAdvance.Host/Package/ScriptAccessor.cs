using System.IO;

namespace DbAdvance.Host.Package
{
    public class ScriptAccessor
    {
        private readonly string scriptPath;

        public ScriptAccessor(string scriptPath)
        {
            this.scriptPath = scriptPath;
        }

        public string Read()
        {
            using (var reader = new StreamReader(scriptPath))
            {
                return reader.ReadToEnd();
            }
        }

        public string GetFullPath()
        {
            return Path.GetFullPath(scriptPath);
        }

        public override string ToString()
        {
            return Path.GetFileName(scriptPath);
        }
    }
}