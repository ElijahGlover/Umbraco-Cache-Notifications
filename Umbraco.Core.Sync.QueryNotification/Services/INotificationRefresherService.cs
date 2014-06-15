using Umbraco.Core.Sync.QueryNotification.Models;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public interface INotificationRefresherService
    {
        void ExecuteAsync(Notification notification);
        void Execute(Notification notification);
        void RefreshAll();
        void RefreshAll(string factoryId);
    }
}