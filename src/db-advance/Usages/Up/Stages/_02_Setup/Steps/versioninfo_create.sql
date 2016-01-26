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