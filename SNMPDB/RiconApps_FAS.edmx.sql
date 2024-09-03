
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/03/2023 14:56:56
-- Generated from EDMX file: C:\Users\zeynep.urtac.SPINTEK2\Desktop\Ricon_FAS\SNMPDB\RiconApps_FAS.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RiconApps_FAS];
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
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Company]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Company];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Device]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Device];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[GsmNumber]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[GsmNumber];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Install]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Install];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Operator]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Operator];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[ReConfig]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[ReConfig];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[RMSC]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[RMSC];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Sites]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Sites];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Tbl_Inwi]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Tbl_Inwi];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[Tbl_Orange]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[Tbl_Orange];
GO
IF OBJECT_ID(N'[RiconApps_FASModelStoreContainer].[UserLogin]', 'U') IS NOT NULL
    DROP TABLE [RiconApps_FASModelStoreContainer].[UserLogin];
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
    [GSM_No] nvarchar(50)  NULL,
    [Site_Name] nvarchar(50)  NULL,
    [WAN_ip] nvarchar(255)  NULL,
    [Operator] nvarchar(50)  NULL,
    [Username] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Date_Time] datetime  NULL
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

-- Creating table 'Sites'
CREATE TABLE [dbo].[Sites] (
    [Site_ID] int IDENTITY(1,1) NOT NULL,
    [Site_Name] nvarchar(255)  NULL,
    [Company_ID] int  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'Tbl_Inwi'
CREATE TABLE [dbo].[Tbl_Inwi] (
    [Inwi_ID] int IDENTITY(1,1) NOT NULL,
    [Ricon_SN] nvarchar(255)  NULL,
    [GSM_No2] nvarchar(255)  NULL,
    [APN2_Name] nvarchar(255)  NULL,
    [APN2_Username] nvarchar(255)  NULL,
    [APN2_Password] nvarchar(255)  NULL,
    [i_WAN_ip] nvarchar(255)  NULL,
    [i_vlanid_TG] nvarchar(255)  NULL,
    [i_lan_ip_TG] nvarchar(255)  NULL,
    [i_lan_subnet_TG] nvarchar(255)  NULL,
    [i_lan_subnetmask_TG] nvarchar(255)  NULL,
    [i_vlanid_Servizi] nvarchar(255)  NULL,
    [i_lan_ip_Servizi] nvarchar(255)  NULL,
    [i_lan_subnet_Servizi] nvarchar(255)  NULL,
    [i_lan_subnetmask_Servizi] nvarchar(255)  NULL,
    [Status] int  NULL,
    [i_Tg_dhcp_start] nvarchar(255)  NULL,
    [i_Tg_dhcp_end] nvarchar(255)  NULL,
    [i_Ser_dhcp_start] nvarchar(255)  NULL,
    [i_Ser_dhcp_end] nvarchar(255)  NULL
);
GO

-- Creating table 'Tbl_Orange'
CREATE TABLE [dbo].[Tbl_Orange] (
    [Orange_ID] int IDENTITY(1,1) NOT NULL,
    [Ricon_SN] nvarchar(255)  NULL,
    [GSM_No1] nvarchar(255)  NULL,
    [APN1_Name] nvarchar(255)  NULL,
    [APN1_Username] nvarchar(255)  NULL,
    [APN1_Password] nvarchar(255)  NULL,
    [WAN_ip] nvarchar(255)  NULL,
    [vlanid_TG] nvarchar(255)  NULL,
    [lan_ip_TG] nvarchar(255)  NULL,
    [lan_subnet_TG] nvarchar(255)  NULL,
    [lan_subnetmask_TG] nvarchar(255)  NULL,
    [vlanid_Servizi] nvarchar(255)  NULL,
    [lan_ip_Servizi] nvarchar(255)  NULL,
    [lan_subnet_Servizi] nvarchar(255)  NULL,
    [lan_subnetmask_Servizi] nvarchar(255)  NULL,
    [Tunnel_dc1_r1] nvarchar(255)  NULL,
    [Tunnel_dc2_r1] nvarchar(255)  NULL,
    [Tunnel_ig_r1] nvarchar(255)  NULL,
    [Status] int  NULL,
    [Tg_dhcp_start] nvarchar(255)  NULL,
    [Tg_dhcp_end] nvarchar(255)  NULL,
    [Ser_dhcp_start] nvarchar(255)  NULL,
    [Ser_dhcp_end] nvarchar(255)  NULL
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

-- Creating table 'GsmNumber'
CREATE TABLE [dbo].[GsmNumber] (
    [GSM_No_ID] int IDENTITY(1,1) NOT NULL,
    [GSM_No] nvarchar(255)  NOT NULL,
    [Status] int  NULL,
    [Company_ID] int  NULL
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

-- Creating primary key on [Site_ID] in table 'Sites'
ALTER TABLE [dbo].[Sites]
ADD CONSTRAINT [PK_Sites]
    PRIMARY KEY CLUSTERED ([Site_ID] ASC);
GO

-- Creating primary key on [Inwi_ID] in table 'Tbl_Inwi'
ALTER TABLE [dbo].[Tbl_Inwi]
ADD CONSTRAINT [PK_Tbl_Inwi]
    PRIMARY KEY CLUSTERED ([Inwi_ID] ASC);
GO

-- Creating primary key on [Orange_ID] in table 'Tbl_Orange'
ALTER TABLE [dbo].[Tbl_Orange]
ADD CONSTRAINT [PK_Tbl_Orange]
    PRIMARY KEY CLUSTERED ([Orange_ID] ASC);
GO

-- Creating primary key on [User_ID] in table 'UserLogin'
ALTER TABLE [dbo].[UserLogin]
ADD CONSTRAINT [PK_UserLogin]
    PRIMARY KEY CLUSTERED ([User_ID] ASC);
GO

-- Creating primary key on [GSM_No_ID], [GSM_No] in table 'GsmNumber'
ALTER TABLE [dbo].[GsmNumber]
ADD CONSTRAINT [PK_GsmNumber]
    PRIMARY KEY CLUSTERED ([GSM_No_ID], [GSM_No] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------