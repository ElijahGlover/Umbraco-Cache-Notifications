using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Umbraco.Core.Persistence;
using Umbraco.Core.Sync.QueryNotification.Extensions;
using Umbraco.Core.Sync.QueryNotification.Services;
using umbraco.interfaces;
using Umbraco.Web.Cache;

namespace Umbraco.Core.Sync.QueryNotification
{
    public class NotificationApplicationEventHandler : IApplicationEventHandler
    {
        private ILogService _logService;
        private IPayloadService _payloadService;
        private Func<Database> _databaseFactory;
        private NotificationRefresherService _refresherService;
        private NotificationReceiverService _receiverService;

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _payloadService = new JsonPayloadService();
            _logService = new LogHelperService();
            _databaseFactory = () => applicationContext.DatabaseContext.Database; //Must Use A Seperate Context For Each Thread - Context Calls UmbracoDatabaseFactory
            _refresherService = new NotificationRefresherService(_payloadService, _logService, new Lazy<IList<ICacheRefresher>>(CacheRefresherResolver.GetRefreshers));
            _receiverService = new NotificationReceiverService(_databaseFactory, _refresherService, _logService);

            var messenger = new NotificationServerMessenger(_databaseFactory, _payloadService, _logService, _receiverService);
            ServerMessengerResolver.Current.SetServerMessenger(messenger);
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) { }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            SqlDependency.Start(applicationContext.DatabaseContext.ConnectionString);
            _refresherService.RefreshAll(DistributedCache.PageCacheRefresherId); //Rebuild Xml Cache
            _receiverService.Start();
        }
    }
}
