using System;
using System.IO;
using System.Linq;

namespace DbAdvance.Host.Package
{
    public class ScriptAccessor
    {
        private readonly string _script;

        public string Tag { get; set; }

        public ScriptAccessor(string script)
        {
            this._script = script;
            ComputeTag();
        }

        public string Read()
        {
            using (var reader = new StreamReader(_script))
            {
                return reader.ReadToEnd();
            }
        }

        public string GetFullPath()
        {
            return Path.GetFullPath(_script);
        }


        public bool HasContent()
        {
            return File.ReadAllText(_script).Trim().Length > 0;
        }

        public override string ToString()
        {
            return Path.GetFileName(_script);
        }

        private void ComputeTag()
        {
            var folders = _script.Split(new string[] {@"\"},
                StringSplitOptions.RemoveEmptyEntries)
                .Reverse()
                .ToList();

            var tag = folders[1];  // folder[0] is the file:
            if (!FolderStructure.Folders.Values.Any(v => v.Equals(tag)))
                Tag = tag;
        }
    }
}