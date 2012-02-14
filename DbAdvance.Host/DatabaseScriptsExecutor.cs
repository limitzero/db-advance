using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DbAdvance.Host.ScriptScanner;

namespace DbAdvance.Host
{
    public class DatabaseScriptsExecutor
    {
        private readonly ILogger log;

        public DatabaseScriptsExecutor(ILogger log)
        {
            this.log = log;
        }

        public void Run(string packageRootPath, IDatabaseScriptsScanner scanner, string connectionString)
        {
            scanner
                .GetScripts(packageRootPath)
                .ForEach(s => ApplyDelta(s, connectionString));
        }

        public void CleanAllDatabasesFromServer(string connectionString)
        {
            log.Log("Removing all databases from server...");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                const string Sql = @"
                    EXEC sp_MSforeachdb '
                    IF (DB_ID(''?'') > 4)
                    BEGIN
                       print (''?'')
                       print (DB_ID(''?''))
                       EXEC (''ALTER DATABASE ? SET SINGLE_USER WITH ROLLBACK IMMEDIATE'')
                       EXEC (''DROP DATABASE ?'')
                    END'
                    ";

                new SqlCommand(Sql, connection)
                    .ExecuteNonQuery();
            }

            log.Log("Done.");
        }

        private static IEnumerable<string> GetDeltaContents(string configurationPath, bool isCommit)
        {
            return
                XDocument.Load(Path.Combine(configurationPath, "index.xml"))
                    .Elements("scripts")
                    .Elements(isCommit
                                  ? "commit"
                                  : "rollback")
                    .Elements("script")
                    .Select(script => (string)script.Attribute("path"))
                    .ToList();
        }

        private static string ReadScript(string scriptPath)
        {
            using (var reader = new StreamReader(scriptPath))
            {
                return reader.ReadToEnd();
            }
        }

        private void ApplyDelta(DatabaseScriptVersion scriptVersion, string connectionString)
        {
            var deltaContents = GetDeltaContents(scriptVersion.Path, scriptVersion.IsCommit);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.InfoMessage += InfoMessage;

                foreach (var script in deltaContents)
                {
                    ExecuteScript(
                        connection,
                        Path.Combine(scriptVersion.Path, script));
                }
            }
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            log.Log(e.Message);
        }

        private void ExecuteScript(SqlConnection connection, string scriptPath)
        {
            log.Log("DB: Executing '{0}'... ", scriptPath);

            var script = ReadScript(scriptPath);

            var commands = Regex.Split(script, @"(?m)^\s*GO\s*\d*\s*$", RegexOptions.IgnoreCase);

            foreach (var c in commands.Where(q => !string.IsNullOrEmpty(q)))
            {
                new SqlCommand(c, connection)
                    .ExecuteNonQuery();
            }

            log.Log("Script Applied");
        }
    }
}