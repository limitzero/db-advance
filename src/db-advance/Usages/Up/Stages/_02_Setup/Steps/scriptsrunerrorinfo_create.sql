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