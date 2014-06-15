using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Sync.QueryNotification.Extensions;
using Umbraco.Core.Sync.QueryNotification.Models;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public class NotificationReceiverService
    {
        private readonly Func<Database> _databaseFactory;
        private readonly INotificationRefresherService _refresherService;
        private readonly ILogService _logService;
        private readonly IDictionary<Guid, DateTime> _processedNotifications = new ConcurrentDictionary<Guid, DateTime>();
        private DateTime _serviceStarted; //TODO Don't Query All Rpc Calls - Refresh Cache On App Start

        private const int NotificationQueryWindow = 1; //1 Minute
        private const int NotificationPurgeWindow = 2; //2 Minutes
        private const int NotificationTimeoutWindow = 1; //1 Minute
        private const int NotificationErrowWindow = 1000; //1 Second

        public NotificationReceiverService(Func<Database> databaseFactory, INotificationRefresherService refresherService, ILogService logService)
        {
            _databaseFactory = databaseFactory;
            _refresherService = refresherService;
            _logService = logService;
        }

        public void Start()
        {
            _serviceStarted = DateTime.UtcNow;
            Task.Factory.StartNew(Process);
        }

        protected void Process()
        {
            var querySql = new Sql()
                .Select("CorrelationId", "FactoryId", "MachineName", "Timestamp", "NotificationType", "Payload")
                .From("[dbo].[RpcNotification]") //Must Specify Schema/Column Names For Query Notifications
                .Where("Timestamp > @timestamp", new { timestamp = DateTime.UtcNow.AddMinutes(-NotificationQueryWindow) });

            try
            {
                var db = _databaseFactory();
                var query = db.QueryWithNotification<Notification>(QueryChanged, NotificationTimeoutWindow, querySql);
                foreach (var notification in query)
                {
                    if (_processedNotifications.ContainsKey(notification.CorrelationId))
                        continue;

                    _refresherService.Execute(notification);
                    _processedNotifications.Add(notification.CorrelationId, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                _logService.Error<NotificationReceiverService>(string.Format("Notification Query Failed, {0}", ex.Message), ex);
                Thread.Sleep(NotificationErrowWindow);
                Process();
                return;
            }

            PurgeNotifications();
        }

        protected void QueryChanged(SqlNotificationEventArgs args)
        {
            if (args.Source == SqlNotificationSource.Timeout || args.Source == SqlNotificationSource.Data)
            {
                Process();
                return;
            }
  
            _logService.Error<NotificationReceiverService>(string.Format("Unexpected Sql Notification {0} By {1}", args.Info, args.Source), new Exception());
            Thread.Sleep(NotificationErrowWindow);
            Process();
        }

        protected void PurgeNotifications()
        {
            var timestamp = DateTime.UtcNow.AddMinutes(-NotificationPurgeWindow);

            try
            {
                var db = _databaseFactory();
                db.Delete<Notification>("WHERE Timestamp < @timestamp", new {timestamp}); //Round To The Minute To Prevent Little
            }
            catch(Exception ex)
            {
                _logService.Error<NotificationReceiverService>("Notifications Purge Failed", ex);
            }

            _processedNotifications.RemoveAll(p => p.Value < timestamp);
        }
    }
}
