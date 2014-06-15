using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Sync.QueryNotification.Models
{
    [TableName("RpcNotification")]
    public class Notification
    {
        public Guid CorrelationId { get; set; }
        public Guid FactoryId { get; set; }
        public string MachineName { get; set; }
        public DateTime Timestamp { get; set; }
        public int NotificationType { get; set; }
        public string Payload { get; set; }
    }
}
