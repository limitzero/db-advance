using System;
using System.Diagnostics;
using System.Globalization;

namespace DbAdvance.Host.DbConnectors
{
    public class SqlCmdRunner
    {
        private readonly ILogger log;

        private const string SqlCmdExe = "sqlcmd.exe";
        private const string SqlCmdParameters = "-S {0} -U {1} -P {2} -i \"{3}\"";

        public SqlCmdRunner(ILogger log)
        {
            this.log = log;
        }

        public void Run(string server, string username, string password, string script)
        {
            var processStartInfo = GetProcessStartInfo(server, username, password, script);

            using (var proc = new Process { StartInfo = processStartInfo })
            {
                proc.Start();

                var output = ReadOutput(proc);

                if (output != string.Empty)
                {
                    log.Log("Exec Output -----------------------------------------------------------------------------------------");
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

        private static ProcessStartInfo GetProcessStartInfo(string server, string username, string password, string script)
        {
           return new ProcessStartInfo(
                SqlCmdExe,
                string.Format(
                    CultureInfo.InvariantCulture,
                    SqlCmdParameters,
                    server,
                    username,
                    password,
                    script))
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