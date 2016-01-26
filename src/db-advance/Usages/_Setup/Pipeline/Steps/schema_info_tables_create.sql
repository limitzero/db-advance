-- create the version info table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VersionInfo]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[VersionInfo]
		(
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[Version] [nvarchar](255) NULL,
			[EntryDate] [datetime] NULL, 
			[EnteredBy] [nvarchar](255) NULL,
			CONSTRAINT PK_VersionInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC )
		) 
	END

-- create the scripts deployment info table:
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunDeployInfo]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[ScriptsRunDeployInfo]
		(
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[VersionInfoId] bigint NULL,
			[DeployPackageName] varchar(max) null, 
			[DeployPackageContent] varbinary(max) null, 
			[DeployedOn] datetime null, 
			[DeployedBy] varchar(255) null, 
		    CONSTRAINT PK_ScriptsRunDeployInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC )
		)
	END

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

-- create the scripts error info table:
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunErrorInfo]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[ScriptsRunErrorInfo]
		(
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[VersionInfoId] bigint NULL,
			[ScriptName] [nvarchar](255) NULL,
			[ScriptText] [text] NULL,
			[ScriptError] [text] NULL,
			[EntryDate] [datetime] NULL,
			CONSTRAINT PK_ScriptRunErrorInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC ), 
			CONSTRAINT FK_ScriptsRunErrorInfo_has_VersionInfo FOREIGN KEY ([VersionInfoId]) REFERENCES [VersionInfo] ([Id])
		)
	END
