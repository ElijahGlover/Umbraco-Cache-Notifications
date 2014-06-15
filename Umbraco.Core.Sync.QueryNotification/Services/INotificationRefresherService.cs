using Umbraco.Core.Sync.QueryNotification.Models;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public interface INotificationRefresherService
    {
        void Execute(Notification notification);
    }
}