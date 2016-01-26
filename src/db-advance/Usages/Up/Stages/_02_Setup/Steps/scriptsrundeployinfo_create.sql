-- create the scripts deployment info table:
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptsRunDeployInfo]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[ScriptsRunDeployInfo]
		(
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[DeployPackageName] varchar(max) null, 
			[DeployPackageContent] varbinary(max) null, 
			[DeployedOn] datetime null, 
			[DeployedBy] varchar(255) null, 
		    CONSTRAINT PK_ScriptsRunDeployInfo PRIMARY KEY NONCLUSTERED ( [Id] ASC )
		)
	END