using System;
using System.Data.SqlClient;
using Umbraco.Core.Persistence;
using Umbraco.Core.Sync.QueryNotification.Extensions;
using Umbraco.Core.Sync.QueryNotification.Services;

namespace Umbraco.Core.Sync.QueryNotification
{
    public class NotificationApplicationEventHandler : IApplicationEventHandler
    {
        private ILogService _logService;
        private IPayloadService _payloadService;
        private Func<Database> _databaseFactory;

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _payloadService = new JsonPayloadService();
            _logService = new LogHelperService();
            _databaseFactory = () => applicationContext.DatabaseContext.Database; //Must Use A Seperate Context For Each Thread - Context Calls UmbracoDatabaseFactory

            var messenger = new NotificationServerMessenger(_databaseFactory, _payloadService, _logService);
            ServerMessengerResolver.Current.SetServerMessenger(messenger);
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) { }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var refresherService = new NotificationRefresherService(_payloadService, _logService, CacheRefresherResolver.GetRefreshers());
            var receiver = new NotificationReceiverService(_databaseFactory, refresherService, _logService);
            SqlDependency.Start(applicationContext.DatabaseContext.ConnectionString);
            receiver.Start();
        }
    }
}
