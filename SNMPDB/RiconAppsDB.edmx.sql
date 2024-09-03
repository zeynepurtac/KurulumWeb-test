
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/27/2023 11:37:37
-- Generated from EDMX file: C:\Users\zeynep.urtac.SPINTEK2\Desktop\Sisal_v50\Mıstafagg\Sisal-Login\SNMPDB\RiconAppsDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RiconAppsDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Groups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Groups];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[Company]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[Company];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[Device]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[Device];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[Install]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[Install];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[Operator]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[Operator];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[ReConfig]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[ReConfig];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[RMSC]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[RMSC];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[SIMCards2]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[SIMCards2];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[SIMCards3]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[SIMCards3];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[Sites]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[Sites];
GO
IF OBJECT_ID(N'[RiconAppsDBModelStoreContainer].[UserLogin]', 'U') IS NOT NULL
    DROP TABLE [RiconAppsDBModelStoreContainer].[UserLogin];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Groups'
CREATE TABLE [dbo].[Groups] (
    [Id] int  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Role] nvarchar(50)  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- Creating table 'Table'
CREATE TABLE [dbo].[Table] (
    [Id] int  NOT NULL,
    [Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Company'
CREATE TABLE [dbo].[Company] (
    [Company_ID] int IDENTITY(1,1) NOT NULL,
    [Company_Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Device'
CREATE TABLE [dbo].[Device] (
    [Device_ID] int IDENTITY(1,1) NOT NULL,
    [Device_Type_ID] int  NULL,
    [Ricon_SN] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'Install'
CREATE TABLE [dbo].[Install] (
    [Install_ID] int IDENTITY(1,1) NOT NULL,
    [Ricon_SN] nvarchar(50)  NULL,
    [GSM_No1] nvarchar(50)  NULL,
    [GSM_No2] nvarchar(50)  NULL,
    [Site_Name] nvarchar(50)  NULL,
    [Username] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Date_Time] datetime  NULL,
    [Default_GSMNo] int  NULL
);
GO

-- Creating table 'Operator'
CREATE TABLE [dbo].[Operator] (
    [Operator_ID] int IDENTITY(1,1) NOT NULL,
    [Operator_Name] nvarchar(50)  NULL
);
GO

-- Creating table 'ReConfig'
CREATE TABLE [dbo].[ReConfig] (
    [RC_ID] int IDENTITY(1,1) NOT NULL,
    [GSM_No] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Site_Name] nvarchar(50)  NULL,
    [DateTime] datetime  NULL
);
GO

-- Creating table 'RMSC'
CREATE TABLE [dbo].[RMSC] (
    [RMS_ID] int IDENTITY(1,1) NOT NULL,
    [Device_Type_ID] int  NULL,
    [RMS_IP] nvarchar(50)  NULL,
    [RMS_Port] nvarchar(50)  NULL,
    [Company_ID] int  NULL
);
GO

-- Creating table 'SIMCards2'
CREATE TABLE [dbo].[SIMCards2] (
    [SIM_ID] int IDENTITY(1,1) NOT NULL,
    [GSM_No1] nvarchar(255)  NULL,
    [APN1_Name] nvarchar(255)  NULL,
    [APN1_Username] nvarchar(255)  NULL,
    [APN1_Password] nvarchar(255)  NULL,
    [GSM_No2] nvarchar(255)  NULL,
    [APN2_Name] nvarchar(255)  NULL,
    [APN2_Username] nvarchar(255)  NULL,
    [APN2_Password] nvarchar(255)  NULL,
    [Lan1_IP] nvarchar(255)  NULL,
    [Lan1_SubnetMask] nvarchar(255)  NULL,
    [Lan_Subnet] nvarchar(255)  NULL,
    [Company_ID] int  NULL,
    [Ricon_SN] nvarchar(255)  NULL
);
GO

-- Creating table 'Sites'
CREATE TABLE [dbo].[Sites] (
    [Site_ID] int IDENTITY(1,1) NOT NULL,
    [Site_Name] nvarchar(255)  NULL,
    [Company_ID] int  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'UserLogin'
CREATE TABLE [dbo].[UserLogin] (
    [User_ID] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(50)  NULL,
    [Password] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [IsAdmin] bit  NULL,
    [Creator] nvarchar(50)  NULL,
    [Status] int  NULL,
    [Create_DateTime] datetime  NULL
);
GO

-- Creating table 'SIMCards3'
CREATE TABLE [dbo].[SIMCards3] (
    [SIM_ID] int IDENTITY(1,1) NOT NULL,
    [GSM_No1] nvarchar(255)  NULL,
    [APN1_Name] nvarchar(255)  NULL,
    [APN1_Username] nvarchar(255)  NULL,
    [APN1_Password] nvarchar(255)  NULL,
    [GSM_No2] nvarchar(255)  NULL,
    [APN2_Name] nvarchar(255)  NULL,
    [APN2_Username] nvarchar(255)  NULL,
    [APN2_Password] nvarchar(255)  NULL,
    [Lan1_IP] nvarchar(255)  NULL,
    [Lan1_SubnetMask] nvarchar(255)  NULL,
    [Lan_Subnet] nvarchar(255)  NULL,
    [Company_ID] int  NULL,
    [Ricon_SN] nvarchar(255)  NULL,
    [Status] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Groups'
ALTER TABLE [dbo].[Groups]
ADD CONSTRAINT [PK_Groups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- Creating primary key on [Id] in table 'Table'
ALTER TABLE [dbo].[Table]
ADD CONSTRAINT [PK_Table]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Company_ID], [Company_Name] in table 'Company'
ALTER TABLE [dbo].[Company]
ADD CONSTRAINT [PK_Company]
    PRIMARY KEY CLUSTERED ([Company_ID], [Company_Name] ASC);
GO

-- Creating primary key on [Device_ID] in table 'Device'
ALTER TABLE [dbo].[Device]
ADD CONSTRAINT [PK_Device]
    PRIMARY KEY CLUSTERED ([Device_ID] ASC);
GO

-- Creating primary key on [Install_ID] in table 'Install'
ALTER TABLE [dbo].[Install]
ADD CONSTRAINT [PK_Install]
    PRIMARY KEY CLUSTERED ([Install_ID] ASC);
GO

-- Creating primary key on [Operator_ID] in table 'Operator'
ALTER TABLE [dbo].[Operator]
ADD CONSTRAINT [PK_Operator]
    PRIMARY KEY CLUSTERED ([Operator_ID] ASC);
GO

-- Creating primary key on [RC_ID] in table 'ReConfig'
ALTER TABLE [dbo].[ReConfig]
ADD CONSTRAINT [PK_ReConfig]
    PRIMARY KEY CLUSTERED ([RC_ID] ASC);
GO

-- Creating primary key on [RMS_ID] in table 'RMSC'
ALTER TABLE [dbo].[RMSC]
ADD CONSTRAINT [PK_RMSC]
    PRIMARY KEY CLUSTERED ([RMS_ID] ASC);
GO

-- Creating primary key on [SIM_ID] in table 'SIMCards2'
ALTER TABLE [dbo].[SIMCards2]
ADD CONSTRAINT [PK_SIMCards2]
    PRIMARY KEY CLUSTERED ([SIM_ID] ASC);
GO

-- Creating primary key on [Site_ID] in table 'Sites'
ALTER TABLE [dbo].[Sites]
ADD CONSTRAINT [PK_Sites]
    PRIMARY KEY CLUSTERED ([Site_ID] ASC);
GO

-- Creating primary key on [User_ID] in table 'UserLogin'
ALTER TABLE [dbo].[UserLogin]
ADD CONSTRAINT [PK_UserLogin]
    PRIMARY KEY CLUSTERED ([User_ID] ASC);
GO

-- Creating primary key on [SIM_ID] in table 'SIMCards3'
ALTER TABLE [dbo].[SIMCards3]
ADD CONSTRAINT [PK_SIMCards3]
    PRIMARY KEY CLUSTERED ([SIM_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------