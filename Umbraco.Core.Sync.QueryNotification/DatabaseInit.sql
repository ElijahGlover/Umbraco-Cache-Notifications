CREATE TABLE [dbo].[RpcNotification](
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[FactoryId] [uniqueidentifier] NOT NULL,
	[MachineName] [nvarchar](100) NULL,
	[Timestamp] [datetime] NOT NULL,
	[NotificationType] [int] NOT NULL,
	[Payload] [nvarchar](4000) NULL,
 CONSTRAINT [PK_RpcNotification]
 PRIMARY KEY CLUSTERED ([CorrelationId] ASC)
 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
 ON [PRIMARY]
) ON [PRIMARY]