using Umbraco.Core.Sync.QueryNotification.Models;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public interface INotificationReceiverService
    {
        void Start();
        void Execute(Notification notification);
    }
}