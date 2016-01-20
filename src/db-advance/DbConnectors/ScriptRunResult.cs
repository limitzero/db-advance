using System.Collections.Generic;
using System.Linq;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.DbConnectors
{
    public class ScriptRunResult
    {
        private IList<string> _errors = new List<string>();
        public ScriptAccessor Script { get; set; }

        public IEnumerable<string> Errors
        {
            get { return _errors; }
        }

        public void RecordError(string error)
        {
            _errors.Add(error);
        }

        public bool HasErrors()
        {
            return Errors.Any();
        }
    }
}