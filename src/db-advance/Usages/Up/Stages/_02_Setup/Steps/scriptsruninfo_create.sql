-- create the script info table:
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunInfo]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[ScriptsRunInfo]
		(
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[VersionInfoId] bigint NULL,
			[Tag] [nvarchar](100) NULL,
			[DeployInfoId] bigint NULL,
			[ScriptName] [nvarchar](255) NULL,
			[ScriptText] [text] NULL,
			[ScriptHash] varbinary(max) NULL,
			[EntryDate] [datetime] NULL,
			CONSTRAINT PK_ScriptRunInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC ), 
			CONSTRAINT FK_ScriptsRunInfo_has_VersionInfo FOREIGN KEY ([VersionInfoId]) REFERENCES [VersionInfo] ([Id]), 
			CONSTRAINT FK_ScriptsRunInfo_has_DeployInfo FOREIGN KEY (DeployInfoId) REFERENCES [ScriptsRunDeployInfo] (Id)
		)
	END