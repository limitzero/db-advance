using System.IO;
using System.Linq;
using Castle.MicroKernel;
using DbAdvance.Host.Commands;
using DbAdvance.Host.Commands.Clean.Pipeline.Steps;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Usages._Setup.Pipeline.Steps
{
    public sealed class CreateSchemaTablesStep
        : BasePipelineStep<CommandPipelineContext>
    {
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DatabaseConnectorFactory _factory;

        public CreateSchemaTablesStep(IKernel kernel,
            IDatabaseConnectorConfiguration configuration,
            DatabaseConnectorFactory factory) : base(kernel)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public override void Execute(CommandPipelineContext context)
        {
            var database = _configuration.GetDatabaseName();
            
            var connector = _factory.UseBasicConnector();
            if (!connector.DatabaseExists())
            {
                Logger.InfoFormat("Target database '{0}' not found on instance '{1}', creating...", 
                    _configuration.GetDatabaseName(), 
                    _configuration.GetDatabaseServerName());

                var step = Pipeline.ResolveStep<CleanDatabaseStep>();
                step.Execute(context);
            }

            Logger.InfoFormat("Inspecting schema on database '{0}' for version and history tables...", database);

            if (context.IsSchemaPresent)
                Logger.InfoFormat("Schema version and history tables present on database '{0}'.", database);
            else
            {
                Logger.InfoFormat("Creating version and history tables on database '{0}'...", database);
                ExtractSchemaDefinitionAndApplyToTargetDatabase();
                //CreateVersionInfoTable();
                //CreateScriptsRunInfoTable();
                //CreateScriptsRunErrorInfoTable();
                //CreateScriptsRunDeployInfoTable();
                Logger.InfoFormat("Version and history tables created on database '{0}'.", database);
            }
        }

        private void ExtractSchemaDefinitionAndApplyToTargetDatabase()
        {
            var schemaDefinitions = ExtractSchemaDefinition();
            var connector = _factory.UseBasicConnector();
            connector.Apply(schemaDefinitions);
        }

        private string ExtractSchemaDefinition()
        {
            var resource = GetType().Assembly.GetManifestResourceNames()
                .FirstOrDefault(rn => rn.EndsWith("schema_info_tables_create.sql"));

            using (var stream = GetType().Assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void CreateVersionInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VersionInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VersionInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](255) NULL,
	[EntryDate] [datetime] NULL, 
	[EnteredBy] [nvarchar](255) NULL,
    CONSTRAINT PK_VersionInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC )
) 
END
";
            var connector = _factory.UseBasicConnector();
            connector.Apply(statement);
        }

        private void CreateScriptsRunInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ScriptsRunInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](255) NULL,
    [Tag] [nvarchar](100) NULL,
    [DeployInfoId] bigint NULL,
	[ScriptName] [nvarchar](255) NULL,
	[ScriptText] [text] NULL,
    [ScriptHash] varbinary(max) NULL,
	[EntryDate] [datetime] NULL,
    CONSTRAINT PK_ScriptRunInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC ), 
    CONSTRAINT FK_ScriptsRunInfo_has_VersionInfo FOREIGN KEY (Version) REFERENCES [VersionInfo] (Version), 
    CONSTRAINT FK_ScriptsRunInfo_has_DeployInfo FOREIGN KEY (DeployInfoId) REFERENCES [ScriptDeployInfo] (Id), 
)
END
";
            var connector = _factory.UseBasicConnector();
            connector.Apply(statement);
        }

        private void CreateScriptsRunErrorInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunErrorInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ScriptsRunErrorInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](255) NULL,
	[ScriptName] [nvarchar](255) NULL,
	[ScriptText] [text] NULL,
    [ScriptError] [text] NULL,
	[EntryDate] [datetime] NULL,
    CONSTRAINT FK_ScriptsRunErrorInfo_has_VersionInfo FOREIGN KEY (Version) REFERENCES [VersionInfo] (Version), 
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
END
";
            var connector = _factory.UseBasicConnector();
            connector.Apply(statement);
        }

        private void CreateScriptsRunDeployInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunDeployInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ScriptsRunDeployInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeployPackageName] varchar(max) null, 
	[DeployPackageContent] varbinary(max) null, 
	[DeployedOn] datetime null, 
	[DeployedBy] varchar(255) null, 
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
END
";
            var connector = _factory.UseBasicConnector();
            connector.Apply(statement);
        }
    }
}