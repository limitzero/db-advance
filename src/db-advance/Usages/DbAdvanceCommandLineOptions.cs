using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.Core.Internal;
using Castle.Core.Logging;
using DbAdvance.Host.Usages.Backup.Pipeline;
using DbAdvance.Host.Usages.Clean.Pipeline;
using DbAdvance.Host.Usages.Create.Pipeline;
using DbAdvance.Host.Usages.Deploy.Pipeline;
using DbAdvance.Host.Usages.Drop.Pipeline;
using DbAdvance.Host.Usages.Help.Pipeline;
using DbAdvance.Host.Usages.Init.Pipeline;
using DbAdvance.Host.Usages.Pack.Pipeline;
using DbAdvance.Host.Usages.Restore.Pipeline;
using DbAdvance.Host.Usages.Up.Stages;

namespace DbAdvance.Host.Usages
{
    public sealed class DbAdvancedOptions
    {
        private readonly ILogger _logger;
        public OptionSet OptionSet { get; private set; }

        public string Command { get; set; }
        public string ScriptsPath { get; set; }

        public string Database { get; set; }
        public string ConnectionString { get; set; }

        public string Version { get; set; }

        public string BackupDirectory { get; set; }
        public string BackupFileName { get; set; }

        public string PackageDirectory { get; set; }
        public string PackageFileName { get; set; }

        public string Environment { get; set; }
        public bool UseSqlCmd { get; set; }
        public bool Warn { get; set; }
        public bool Wait { get; set; }
        
        public DbAdvancedOptions(ILogger logger)
        {
            _logger = logger;
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
            Command = "up";
        }

        public void ConfigureForSetup()
        {
            Command = "setup";
        }

        private void BuildCommands(OptionSet options)
        {
            options
                .Add(string.Join("|", HelpPipeline.CommandAliases), option =>
                {
                    SetCommand(HelpPipeline.CommandAliases);
                })
                .Add(string.Join("|", CreateDatabasePipeline.CommandAliases), option =>
                {
                    SetCommand(CreateDatabasePipeline.CommandAliases);
                })
                .Add(string.Join("|", DropDatabasePipeline.CommandAliases), option =>
                {
                    SetCommand(DropDatabasePipeline.CommandAliases);
                })
                .Add(string.Join("|", UpPipeline.CommandAliases), option =>
                {
                    SetCommand(UpPipeline.CommandAliases);
                })
                .Add(string.Join("|", CleanDatabasePipeline.CommandAliases), option =>
                {
                    SetCommand(CleanDatabasePipeline.CommandAliases);
                })
                .Add(string.Join("|", BackupDatabasePipeline.CommandAliases), option =>
                {
                    SetCommand(BackupDatabasePipeline.CommandAliases);
                })
                .Add(string.Join("|", RestoreDatabasePipeline.CommandAliases), option =>
                {
                    SetCommand(RestoreDatabasePipeline.CommandAliases);
                })
                .Add(string.Join("|", PackagePipeline.CommandAliases), option =>
                {
                    SetCommand(PackagePipeline.CommandAliases);
                })
                .Add(string.Join("|", DeployPipeline.CommandAliases), option =>
                {
                    SetCommand(DeployPipeline.CommandAliases);
                })
                .Add(string.Join("|", InitializePipeline.CommandAliases), option =>
                {
                    SetCommand(InitializePipeline.CommandAliases);
                });
        }

        private void BuildSwitches(OptionSet options)
        {
            options
                .Add("database=|db=",
                    option => { Database = option; })
                .Add("connection|cn=",
                    option => { ConnectionString = option; })
                .Add("scriptsPath=|sp=",
                    option => { ScriptsPath = string.IsNullOrEmpty(option) ? System.Environment.CurrentDirectory : option; })
                .Add("version=|v=",
                    option => { Version = string.IsNullOrEmpty(option) ? string.Empty : option; })
                .Add("backupDir=|bd=",
                    option => { BackupDirectory = string.IsNullOrEmpty(option) ? string.Empty : option; })
                .Add("backupFile=|bf=",
                    option => { BackupFileName = string.IsNullOrEmpty(option) ? string.Empty : option; })
                 .Add("packageDir=|pd=",
                    option => { PackageDirectory = string.IsNullOrEmpty(option) ? string.Empty : option; })
                .Add("packageFile=|pf=",
                    option => { PackageFileName = string.IsNullOrEmpty(option) ? string.Empty : option; })
                .Add("warn", option => { Warn = true; })
                .Add("env=|e=",
                    option => { Environment = string.IsNullOrEmpty(option) ? string.Empty : option;  })
                .Add("wait|w",
                    option => { Wait = true; });
        }

        private void SetCommand(IEnumerable<string> commands )
        {
            if (string.IsNullOrEmpty(Command))
                Command = commands.First();
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