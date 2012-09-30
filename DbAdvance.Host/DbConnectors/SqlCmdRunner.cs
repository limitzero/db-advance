using System;
using System.Diagnostics;
using System.Globalization;

namespace DbAdvance.Host.DbConnectors
{
    public class SqlCmdRunner
    {
        private readonly ILogger log;

        private const string SqlCmdExe = "sqlcmd.exe";

        public SqlCmdRunner(ILogger log)
        {
            this.log = log;
        }

        public void Run(string server, string username, string password, string script, string databaseName = null)
        {
            var processStartInfo = GetProcessStartInfo(server, username, password, script, databaseName);

            using (var proc = new Process { StartInfo = processStartInfo })
            {
                proc.Start();

                var output = ReadOutput(proc);

                if (output != string.Empty)
                {
                    log.Log("-----------------------------------------------------------------------------------------------------");
                    log.Log(output);
                    log.Log("-----------------------------------------------------------------------------------------------------");
                }

                var exitCode = proc.ExitCode;
                proc.Close();

                if (exitCode != 0)
                {
                    throw new InvalidOperationException("SqlCmd returned error.");
                }
            }
        }

        private static ProcessStartInfo GetProcessStartInfo(string server, string username, string password, string script, string databaseName)
        {
            var command = string.IsNullOrEmpty(username)
                ? string.Format(CultureInfo.InvariantCulture, "-S {0} -i \"{1}\"", server, script)
                : string.Format(CultureInfo.InvariantCulture, "-S {0} -U {1} -P {2} -i \"{3}\"", server, username, password, script);

            if (databaseName != null)
            {
                command += string.Format(" -d {0}", databaseName);
            }

            return new ProcessStartInfo(
                SqlCmdExe,
                command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
        }

        private static string ReadOutput(Process proc)
        {
            var standardOutput = proc.StandardOutput.ReadToEnd();
            var errorOutput = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            return standardOutput + errorOutput;
        }
    }
}