USE [master]
GO
/****** Object:  Database [PollingData]    Script Date: 2015-07-18 21:42:54 ******/
CREATE DATABASE [PollingData]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'PollingData', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\PollingData.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'PollingData_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\PollingData_log.ldf' , SIZE = 35712KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [PollingData] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PollingData].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PollingData] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [PollingData] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [PollingData] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [PollingData] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [PollingData] SET ARITHABORT OFF 
GO
ALTER DATABASE [PollingData] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [PollingData] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [PollingData] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [PollingData] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [PollingData] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [PollingData] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [PollingData] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [PollingData] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [PollingData] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [PollingData] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [PollingData] SET  DISABLE_BROKER 
GO
ALTER DATABASE [PollingData] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [PollingData] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [PollingData] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [PollingData] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [PollingData] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [PollingData] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [PollingData] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [PollingData] SET RECOVERY FULL 
GO
ALTER DATABASE [PollingData] SET  MULTI_USER 
GO
ALTER DATABASE [PollingData] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [PollingData] SET DB_CHAINING OFF 
GO
ALTER DATABASE [PollingData] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [PollingData] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'PollingData', N'ON'
GO
USE [PollingData]
GO
/****** Object:  User [Developer]    Script Date: 2015-07-18 21:42:55 ******/
CREATE USER [Developer] FOR LOGIN [Developer] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [Developer]
GO
/****** Object:  Table [dbo].[Kodi_CrSessionVideos]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Kodi_CrSessionVideos](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[SessionID] [bigint] NOT NULL,
	[VideoID] [bigint] NOT NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Kodi_CrSessionVideos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Kodi_VideoSessions]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Kodi_VideoSessions](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Kodi_VideoSessions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Kodi_ViewedVideos]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Kodi_ViewedVideos](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [varchar](100) NULL,
	[Title] [varchar](max) NULL,
	[Showtitle] [varchar](max) NULL,
	[Label] [varchar](max) NULL,
	[Runtime] [int] NULL,
	[Season] [int] NULL,
	[Episode] [int] NULL,
	[Thumbnail] [varchar](max) NULL,
 CONSTRAINT [PK_Kodi_ViewedVideos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_GameAchievements]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_GameAchievements](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [bigint] NOT NULL,
	[GameID] [int] NOT NULL,
	[SessionID] [bigint] NULL,
	[Time] [datetime] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Achieved] [bit] NOT NULL,
 CONSTRAINT [PK_Steam_GameAchivements] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_GameStats]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_GameStats](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [bigint] NOT NULL,
	[GameID] [int] NOT NULL,
	[SessionID] [bigint] NULL,
	[Time] [datetime] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_Steam_GameStatistics] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_GamingSessions]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_GamingSessions](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [bigint] NOT NULL,
	[GameID] [int] NULL,
	[GameName] [varchar](255) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Steam_GamingSessions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_ReportFilters]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_ReportFilters](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[SetID] [bigint] NOT NULL,
	[GroupID] [int] NOT NULL,
	[GroupRule] [varchar](50) NOT NULL,
	[Attribute] [varchar](100) NOT NULL,
	[Operator] [varchar](100) NOT NULL,
	[Value] [varchar](max) NULL,
 CONSTRAINT [PK_Steam_ReportFilters] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_ReportFilterSets]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_ReportFilterSets](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Tag] [varchar](50) NULL,
 CONSTRAINT [PK_Steam_ReportFilterSets] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_Reports]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_Reports](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[FilterSetID] [bigint] NOT NULL,
	[UserID] [bigint] NULL,
	[ReferringSubscriptionID] [bigint] NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](max) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[LastGenerated] [datetime] NULL,
 CONSTRAINT [PK_Steam_Reports] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_ReportSessions]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Steam_ReportSessions](
	[ReportID] [bigint] NOT NULL,
	[SessionID] [bigint] NOT NULL,
 CONSTRAINT [PK_Steam_ReportSessions] PRIMARY KEY CLUSTERED 
(
	[ReportID] ASC,
	[SessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Steam_ReportSubscriptions]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_ReportSubscriptions](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [bigint] NOT NULL,
	[TemplateID] [bigint] NOT NULL,
	[Enabled] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[PeriodType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Steam_ReportSubscriptions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Steam_ReportTemplates]    Script Date: 2015-07-18 21:42:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Steam_ReportTemplates](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[FilterSetID] [bigint] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](max) NULL,
	[TimeCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[Deleted] [bit] NOT NULL,
 CONSTRAINT [PK_Steam_ReportTemplates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Kodi_CrSessionVideos]  WITH CHECK ADD  CONSTRAINT [FK_Kodi_CrSessionVideos_Kodi_VideoSessions] FOREIGN KEY([SessionID])
REFERENCES [dbo].[Kodi_VideoSessions] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Kodi_CrSessionVideos] CHECK CONSTRAINT [FK_Kodi_CrSessionVideos_Kodi_VideoSessions]
GO
ALTER TABLE [dbo].[Kodi_CrSessionVideos]  WITH CHECK ADD  CONSTRAINT [FK_Kodi_CrSessionVideos_Kodi_ViewedVideos] FOREIGN KEY([VideoID])
REFERENCES [dbo].[Kodi_ViewedVideos] ([ID])
GO
ALTER TABLE [dbo].[Kodi_CrSessionVideos] CHECK CONSTRAINT [FK_Kodi_CrSessionVideos_Kodi_ViewedVideos]
GO
ALTER TABLE [dbo].[Steam_ReportFilters]  WITH CHECK ADD  CONSTRAINT [FK_Steam_ReportFilters_Steam_ReportFilterSets] FOREIGN KEY([SetID])
REFERENCES [dbo].[Steam_ReportFilterSets] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Steam_ReportFilters] CHECK CONSTRAINT [FK_Steam_ReportFilters_Steam_ReportFilterSets]
GO
ALTER TABLE [dbo].[Steam_Reports]  WITH CHECK ADD  CONSTRAINT [FK_Steam_Reports_Steam_ReportFilterSets] FOREIGN KEY([FilterSetID])
REFERENCES [dbo].[Steam_ReportFilterSets] ([ID])
GO
ALTER TABLE [dbo].[Steam_Reports] CHECK CONSTRAINT [FK_Steam_Reports_Steam_ReportFilterSets]
GO
ALTER TABLE [dbo].[Steam_Reports]  WITH CHECK ADD  CONSTRAINT [FK_Steam_Reports_Steam_ReportSubscriptions] FOREIGN KEY([ReferringSubscriptionID])
REFERENCES [dbo].[Steam_ReportSubscriptions] ([ID])
GO
ALTER TABLE [dbo].[Steam_Reports] CHECK CONSTRAINT [FK_Steam_Reports_Steam_ReportSubscriptions]
GO
ALTER TABLE [dbo].[Steam_ReportSessions]  WITH CHECK ADD  CONSTRAINT [FK_Steam_ReportSessions_Steam_GamingSessions] FOREIGN KEY([SessionID])
REFERENCES [dbo].[Steam_GamingSessions] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Steam_ReportSessions] CHECK CONSTRAINT [FK_Steam_ReportSessions_Steam_GamingSessions]
GO
ALTER TABLE [dbo].[Steam_ReportSessions]  WITH CHECK ADD  CONSTRAINT [FK_Steam_ReportSessions_Steam_Reports] FOREIGN KEY([ReportID])
REFERENCES [dbo].[Steam_Reports] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Steam_ReportSessions] CHECK CONSTRAINT [FK_Steam_ReportSessions_Steam_Reports]
GO
ALTER TABLE [dbo].[Steam_ReportSubscriptions]  WITH CHECK ADD  CONSTRAINT [FK_Steam_ReportSubscriptions_Steam_ReportTemplates] FOREIGN KEY([TemplateID])
REFERENCES [dbo].[Steam_ReportTemplates] ([ID])
GO
ALTER TABLE [dbo].[Steam_ReportSubscriptions] CHECK CONSTRAINT [FK_Steam_ReportSubscriptions_Steam_ReportTemplates]
GO
ALTER TABLE [dbo].[Steam_ReportTemplates]  WITH CHECK ADD  CONSTRAINT [FK_Steam_ReportTemplates_Steam_ReportFilterSets] FOREIGN KEY([FilterSetID])
REFERENCES [dbo].[Steam_ReportFilterSets] ([ID])
GO
ALTER TABLE [dbo].[Steam_ReportTemplates] CHECK CONSTRAINT [FK_Steam_ReportTemplates_Steam_ReportFilterSets]
GO
USE [master]
GO
ALTER DATABASE [PollingData] SET  READ_WRITE 
GO
