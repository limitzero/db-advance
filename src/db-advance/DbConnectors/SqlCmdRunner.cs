using System;
using System.Diagnostics;
using System.Globalization;
using Castle.Core.Logging;

namespace DbAdvance.Host.DbConnectors
{
    public class SqlCmdRunner
    {
        private readonly ILogger _logger;

        private const string SqlCmdExe = "sqlcmd.exe";

        public SqlCmdRunner(ILogger logger)
        {
            _logger = logger;
        }

        public string Run(string server, string username, string password, string script, string databaseName = null)
        {
            var output = string.Empty;
            var processStartInfo = GetProcessStartInfo(server, username, password, script, databaseName);

            using (var proc = new Process {StartInfo = processStartInfo})
            {
                proc.Start();

                output = ReadOutput(proc);

                var exitCode = proc.ExitCode;
                proc.Close();

                if (exitCode != 0)
                {
                    throw new InvalidOperationException("SqlCmd returned error.");
                }
            }

            return output;
        }

        private static ProcessStartInfo GetProcessStartInfo(string server, string username, string password,
            string script, string databaseName)
        {
            var command = string.IsNullOrEmpty(username)
                ? string.Format(CultureInfo.InvariantCulture, "-S {0} -i \"{1}\"", server, script)
                : string.Format(CultureInfo.InvariantCulture, "-S {0} -U {1} -P {2} -i \"{3}\"", server, username,
                    password, script);

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