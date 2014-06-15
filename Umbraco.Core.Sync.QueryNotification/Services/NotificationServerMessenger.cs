using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Sync.QueryNotification.Models;
using umbraco.interfaces;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public class NotificationServerMessenger : ServerMessengerBase
    {
        private readonly Func<Database> _databaseFactory;
        private readonly INotificationReceiverService _receiverService;
        private readonly IPayloadService _payloadService;
        private readonly ILogService _logService;

        public NotificationServerMessenger(Func<Database> databaseFactory, IPayloadService payloadService, ILogService logService, INotificationReceiverService receiverService)
        {
            _databaseFactory = databaseFactory;
            _payloadService = payloadService;
            _logService = logService;
            _receiverService = receiverService;
        }

        public override void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload)
        {
            if (refresher == null || string.IsNullOrWhiteSpace(jsonPayload))
                return;
            Execute(refresher.UniqueIdentifier, MessageType.RefreshByJson, jsonPayload);
        }

        public override void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (refresher == null || numericIds == null || numericIds.Length ==  0)
                return;
            Execute(refresher.UniqueIdentifier, MessageType.RemoveById, _payloadService.Serialize(numericIds));
        }

        public override void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (refresher == null || numericIds == null || numericIds.Length == 0)
                return;
            Execute(refresher.UniqueIdentifier, MessageType.RefreshById, _payloadService.Serialize(numericIds));
        }

        public override void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds)
        {
            if (refresher == null || guidIds == null || guidIds.Length == 0)
                return;
            Execute(refresher.UniqueIdentifier, MessageType.RefreshById, _payloadService.Serialize(guidIds));
        }

        public override void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher)
        {
            if (refresher == null)
                return;
            Execute(refresher.UniqueIdentifier, MessageType.RefreshAll);
        }

        private void Execute(Guid cacheRefresherId, MessageType messageType, string payload = null)
        {
            var notification = new Notification
            {
                CorrelationId = Guid.NewGuid(),
                FactoryId = cacheRefresherId,
                NotificationType = (int) messageType,
                MachineName = Environment.MachineName,
                Payload = payload,
                Timestamp = DateTime.UtcNow
            };

            var db = _databaseFactory();
            db.Insert(notification);
            _logService.Info<NotificationServerMessenger>(string.Format("Notification Created With {0} {1}", notification.CorrelationId, messageType));
            _receiverService.Execute(notification); //Execute Notification Synchronously
        }
    }
}
