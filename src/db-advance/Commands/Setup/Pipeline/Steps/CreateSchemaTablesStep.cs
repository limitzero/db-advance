using Castle.MicroKernel;
using DbAdvance.Host.DbConnectors;
using DbAdvance.Host.Pipeline;

namespace DbAdvance.Host.Commands.Setup.Pipeline.Steps
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

            Logger.WriteBanner();

            Logger.Info("STAGE: Inspect target schema for script and history tables");

            Logger.InfoFormat("Inspecting schema on database '{0}'...", database);

            if (context.IsSchemaPresent)
                Logger.InfoFormat("Schema tables present on database '{0}'.", database);
            else
            {
                Logger.InfoFormat("Creating schema tables on database '{0}'...", database);
                CreateVersionInfoTable();
                CreateScriptsRunInfoTable();
                CreateScriptsRunErrorInfoTable();
                CreateScriptsRunDeployInfoTable();
                Logger.InfoFormat("Schema tables created on database '{0}'.", database);
            }

            Logger.WriteBanner();
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

        private void CreateScriptsRunInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ScriptsRunInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](100) NULL,
    [Tag] [nvarchar](100) NULL,
    [DeployInfoId] bigint NULL,
    [IsRollbackScript] bit NULL,
	[ScriptName] [nvarchar](255) NULL,
	[ScriptText] [text] NULL,
    [ScriptHash] varbinary(max) NULL,
	[EntryDate] [datetime] NULL,
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

        private void CreateScriptsRunErrorInfoTable()
        {
            const string statement = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunErrorInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ScriptsRunErrorInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](100) NULL,
	[ScriptName] [nvarchar](255) NULL,
	[ScriptText] [text] NULL,
    [ScriptError] [text] NULL,
	[EntryDate] [datetime] NULL,
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