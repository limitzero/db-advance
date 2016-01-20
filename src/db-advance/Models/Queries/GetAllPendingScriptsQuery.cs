using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using DbAdvance.Host.Models.Entities;
using DbAdvance.Host.Package;

namespace DbAdvance.Host.Models.Queries
{
    public sealed class GetAllPendingScriptsQuery : BaseQuery<IEnumerable<string>>
    {
        public IEnumerable<string> FoundScripts { get; set; }
        public bool UseOneTimeOnly { get; set; }

        public override IEnumerable<string> Execute(SqlConnection connection)
        {
            var scripts = new List<string>();

            var statement = string.Format(
                "select scripts.* from [{0}] scripts",
                ScriptsRunInfo.GetTableName());

            var executedScripts =
                connection
                    .Query<ScriptsRunInfo>(statement)
                    .Select(si => new ScriptsRunInfo {ScriptName = si.ScriptName})
                    .Distinct()
                    .ToList();

            if (executedScripts.Any())
            {
                scripts.AddRange(FindScriptsThatHaveNotBeenExecutedBefore(executedScripts));
                scripts.AddRange(FindScriptsThatHaveExecutedButChanged(connection, executedScripts));
            }
            else
            {
                scripts.AddRange(FoundScripts);
            }

            return scripts
                .Distinct()
                .ToList();
        }

        private IEnumerable<string> FindScriptsThatHaveNotBeenExecutedBefore(
            IEnumerable<ScriptsRunInfo> executedScripts)
        {
            var scripts = new List<string>();
            foreach (var script in FoundScripts)
            {
                if (executedScripts.Any(s =>
                    string.Compare(s.ToString(),
                        Path.GetFileName(script),
                        CultureInfo.InvariantCulture,
                        CompareOptions.IgnoreCase) > 0))
                    continue;
                scripts.Add(script);
            }

            return scripts;
        }

        private IEnumerable<string> FindScriptsThatHaveExecutedButChanged(
            SqlConnection connection,
            IEnumerable<ScriptsRunInfo> executedScripts)
        {
            var scripts = new List<string>();

            foreach (var script in FoundScripts)
            {
                if (executedScripts.Any(s =>
                    string.Compare(s.ToString(),
                        Path.GetFileName(script),
                        CultureInfo.InvariantCulture,
                        CompareOptions.IgnoreCase) < 0))
                    continue;
                scripts.Add(script);
            }

            var changed = (from found in scripts
                from executed in executedScripts
                where ContentHasChanged(connection, executed, found)
                select found)
                .Distinct()
                .ToList();

            scripts.AddRange(changed);
            return scripts;
        }


        private bool FileNamesAreNotTheSame(ScriptsRunInfo executedScript, string foundScript)
        {
            var foundScriptFileName = Path.GetFileName(foundScript);
            var areNotSameFileName = executedScript.ScriptName.ToLower() != foundScriptFileName.ToLower();
            return areNotSameFileName;
        }

        private bool ContentHasChanged(
            SqlConnection connection,
            ScriptsRunInfo recordedScript,
            string foundScript)
        {
            var found = new ScriptAccessor(foundScript);
            var foundInfo = new ScriptsRunInfo {ScriptText = found.Read()};
            return foundInfo.ScriptHash != recordedScript.ScriptHash;
        }
    }
}