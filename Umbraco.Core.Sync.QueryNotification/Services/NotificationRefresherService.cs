﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Sync.QueryNotification.Extensions;
using Umbraco.Core.Sync.QueryNotification.Models;
using umbraco.interfaces;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public class NotificationRefresherService : INotificationRefresherService
    {
        private readonly IPayloadService _payloadService;
        private readonly ILogService _logService;
        private readonly IList<ICacheRefresher> _cacheRefreshers;
        private readonly ThreadedQueue<Notification> _queue = new ThreadedQueue<Notification>();

        public NotificationRefresherService(IPayloadService payloadService, ILogService logService, IEnumerable<ICacheRefresher> cacheRefreshers)
        {
            _payloadService = payloadService;
            _logService = logService;
            _cacheRefreshers = cacheRefreshers.ToList();
            _queue.OnEnqueue += Process;
            _queue.OnError += ProcessNotificationException;
        }

        public void Execute(Notification notification)
        {
            if (notification == null)
                return;

            _queue.Enqueue(notification);
        }

        void ProcessNotificationException(object sender, Exception exception)
        {
            _logService.Error<NotificationRefresherService>("Exception Executing Notification", exception);
        }

        void Process(object sender, Notification notification)
        {
            var refresher = _cacheRefreshers.FirstOrDefault(p => p.UniqueIdentifier == notification.FactoryId);
            if (refresher == null)
                throw new NotSupportedException(string.Format("Cache Refresher Not Found With Id '{0}'", notification.FactoryId));

            var type = (MessageType)notification.NotificationType;
            if (type == MessageType.RefreshAll)
                ProcessRefreshAll(refresher);
            else if (type == MessageType.RefreshById || type == MessageType.RefreshByInstance)
                ProcessRefreshById(notification, refresher);
            else if (type == MessageType.RemoveById || type == MessageType.RemoveByInstance)
                ProcessRemoveById(notification, refresher);
            else if (type == MessageType.RefreshByJson)
                ProcessRefreshJson(notification, refresher);

            var date = DateTime.UtcNow - notification.Timestamp;
            _logService.Info<NotificationRefresherService>(() => string.Format("Received Notification In {0}ms, Id: {1}, Type:{2}, Payload: {3}", date.Milliseconds, notification.CorrelationId, type, notification.Payload));
        }

        void ProcessRefreshById(Notification notification, ICacheRefresher refresher)
        {
            foreach (var id in _payloadService.Deserialize<object>(notification.Payload))
            {
                if (id is long)
                    refresher.Refresh(Convert.ToInt32((long)id));

                if (id is Guid)
                    refresher.Refresh((Guid)id);
            }
        }

        void ProcessRefreshAll(ICacheRefresher refresher)
        {
            refresher.RefreshAll();
        }

        void ProcessRemoveById(Notification notification, ICacheRefresher refresher)
        {
            foreach (var id in _payloadService.Deserialize<int>(notification.Payload))
                refresher.Remove(id);
        }

        void ProcessRefreshJson(Notification notification, ICacheRefresher refresher)
        {
            //Tricky As IJsonCacheRefresher Is Marked As Internal
            var refresh = refresher.GetType().GetMethod("Refresh", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
            if (refresh == null)
                throw new NotSupportedException("Cache Refresher Does Not Implement IJsonCacheRefresher");
            refresh.Invoke(refresher, new object[] { notification.Payload });
        }
    }
}