using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Castle.Core.Internal;
using Castle.Core.Logging;

namespace DbAdvance.Host
{
    public sealed class DbAdvanceCommandLineOptions
    {
        private readonly ILogger _logger;
        public OptionSet OptionSet { get; private set; }
        public string Command { get; private set; }
        public bool Install { get; set; }
        public bool Rollback { get; set; }
        public string Database { get; set; }
        public string Path { get; set; }
        public string Environment { get; set; }
        public string PackagePath { get; set; }
        public string PackageName { get; set; }
        public int VersionsToRollback { get; set; }
        public string Version { get; set; }
        public bool UseSqlCmdUtility { get; set; }
        public bool Wait { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public DbAdvanceCommandLineOptions(ILogger logger)
        {
            _logger = logger;
            UseSqlCmdUtility = false;
            Tags = new List<string>();
        }

        public void Configure(string[] args)
        {
            var options = new OptionSet();

            BuildCommands(options);
            BuildSwitches(options);

            OptionSet = options;
            options.Parse(args);
        }

        public void ConfigureForUp()
        {
            Install = true;
            Command = "up";
        }

        public void ConfigureForSetup()
        {
            Command = "setup";
        }

        private void BuildCommands(OptionSet options)
        {
            options
                .Add("?|h|help", "Shows help for utility", o => ShowHelp())
                .Add("up",
                    BuildDescription("Apply the current set of deltas to the target database from the 'up' folder.",
                        string.Empty,
                        "Usage: ",
                        "db-advance.exe --up --database=(database alias from connection strings in config) --path=(your root directory for scripts)",
                        string.Empty,
                        "Example:",
                        @" db-advance.exe --up --database=Northwind --path='C:\Databases\Northwind'",
                        string.Empty
                        ),
                    option => { ConfigureForUp(); })
                .Add("down",
                    BuildDescription(
                        "Reverse a series of deltas to the target database as specified from the 'down' folder.",
                        string.Empty,
                        "Usage: ",
                        "db-advance.exe --down --database=(database alias from connection strings in config) --path=(your root directory for scripts) --steps=(number of revisions to revert back)",
                        string.Empty,
                        "Example:",
                        @" db-advance.exe --down --database=Northwind --path='C:\Databases\Northwind' --steps=1",
                        string.Empty
                        ),
                    option =>
                    {
                        Rollback = true;
                        Command = "down";
                    })
                .Add("pack",
                    BuildDescription("Packages any pending scripts for deployment from a target database.",
                        string.Empty,
                        "Usage: db-advance.exe --pack  --database=(database alias from connection strings in config) --path=(your root directory for scripts) --package=(name of your *.zip file)",
                        string.Empty
                        ),
                    option => { Command = "pack"; })
                .Add("deploy",
                    BuildDescription(
                        "Used to deploy a previously zipped archive of the scripts for applying to a target database. ",
                        "The deploy process will issue an 'up' command after the contents are extracted and change the path switch (--path) to the directory where the package was extracted.",
                        "This is used in conjunction with the pack command (--pack) for creating the archive to deploy",
                        string.Empty,
                        "Usage: db-advance.exe --deploy --database=Northwind --package=db-advance.zip",
                        string.Empty),
                    option => { Command = "deploy"; })
                .Add("init",
                    BuildDescription(
                        "Used to create the default folder structure for housing the scripts for execution.",
                        "This is used in conjunction with the path switch (--path,--p)  to specify the directory path where the folders will be created.",
                        string.Empty,
                        "Usage: ",
                        "db-advance.exe --init --path=(your directory for scripts)",
                        string.Empty,
                        "Example:",
                        @"db-advance.exe --init --path='C:\Databases\Nortwind\Scripts'",
                        string.Empty),
                    option => { Command = "init"; })
                .Add("rebuild|refresh|rb",
                    BuildDescription(
                        "Cleans the target database and apply all scripts against it.",
                        string.Empty,
                        "Usage: db-advance.exe --rebuild --database=Northwind --path={your directory for scripts}",
                        string.Empty,
                        "Example:",
                        @"db-advance.exe --rebuild --database=Northwind --path='C:\Databases\Nortwind\Scripts'",
                        string.Empty),
                    option =>
                    {
                        Command = "rebuild";
                    })
                .Add("clean",
                    BuildDescription(
                        "Used to remove all schema objects in the target database (please note that this also includes the history tables).",
                        string.Empty,
                        "Usage: db-advance.exe --clean --database=Northwind",
                        string.Empty),
                    option =>
                    {
                        Command = "clean";
                    });
        }

        private void BuildSwitches(OptionSet options)
        {
            options
                .Add("database=|db=",
                    "(Required) Name of the connection to the target database from the configuration file",
                    option => { Database = option; })
                .Add("path=|p=",
                    "The directory where the scripts are located for execution against the target database. Defaults to executable path.",
                    option => { Path = string.IsNullOrEmpty(option) ? System.Environment.CurrentDirectory : option; })
                .Add("env=|e=",
                    BuildDescription(
                        "Desired environment for running specific scripts that are formatted as {environment}.filename.env.sql"),
                    option => { Environment = option; })
                .Add("version=|v=",
                    BuildDescription(
                        "Desired version to set the database to after scripts are applied successfully (i.e. deterministic versioning).",
                        "If the version is not supplied, it will default to the next highest version number in the versions table (i.e. non-deterministic version).",
                        "Non-determinstic versioning should only be used in local development."),
                    option => { Version = string.IsNullOrEmpty(option) ? string.Empty : option; })
                .Add("steps=|s=",
                    "Desired number of versions to revert in the target database for rollback or down-grade.",
                    option =>
                    {
                        int versionsToRollback = 0;
                        Int32.TryParse(option, out versionsToRollback);
                        VersionsToRollback = versionsToRollback;
                    })
                .Add("tags=|ts=",
                    "Desired comma-separated series of tags of the specific scripts to use when down-grading or rolling back a change or series of changes on the target database. If not specified all rollbacks for a version will be applied.",
                    option =>
                    {
                        if (!string.IsNullOrEmpty(option))
                        {
                            var tags = option.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                            Tags = tags;
                        }
                    })
                .Add("package=|pn=",
                    BuildDescription(
                        "Name of the file that will be created as a zip archive to hold all of the pending scripts for execution (default is is db-advance-yyyyMMdd-HHmmss.zip).",
                        "This will be used in conjuction with the --deploy command to un-zip the contents and run all changes."
                        ),
                    option =>
                    {
                        if (!string.IsNullOrEmpty(option))
                            PackageName = option;
                    })
                .Add("sqlcmd",
                    "Flag to indicate that the scripts should be executed with the SQLCMD utility (defaults to native ADO.NET with transactions)",
                    option =>
                    {
                            UseSqlCmdUtility = true;
                    })
                .Add("wait|w",
                    BuildDescription(
                        "Wait for prompt to for continuing after actions are conducted against the target database."
                        ),
                    option => { Wait = true; });
        }

        public string GetDefaultPackageName()
        {
            var name = string.Format("db-advance-{0}-{1}.zip",
                System.DateTime.Now.ToString("yyyyMMdd"),
                System.DateTime.Now.ToString("HHmmss"));
            return name;
        }

        private string BuildDescription(params string[] lines)
        {
            var description = new StringBuilder();
            lines.ForEach(line => description.AppendLine(line));
            return description.ToString();
        }

        private void ShowHelp()
        {
            const string usage = @"
db-advance.exe [options]

-- upgrade or install a set of changes to the target database:
db-advance.exe --up --database=Northwind --path=(root path to scripts)

-- downgrade or rollback a set of changes to the target database:
db-advance.exe --down --database=Northwind --path=(root path to scripts) --steps=1 

-- package a series of pending scripts for deployment (default is db-advance-package.zip)
db-advance.exe --pack  --path=(root path to scripts) --package=(name of your package file)

--un-package a series of pending scripts for deployment
db-advance.exe --deploy --database=Northwind  --package=(name of your package file)
";
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                OptionSet.WriteOptionDescriptions(writer);
                var content = writer.ToString();
                Console.WriteLine("{0}{1}{2}", usage, System.Environment.NewLine, content);
            }

            if (System.Environment.UserInteractive)
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
            }
            System.Environment.Exit(0);
        }
    }
}