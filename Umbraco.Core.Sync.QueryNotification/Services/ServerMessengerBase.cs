using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.interfaces;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public abstract class ServerMessengerBase : IServerMessenger
    {
        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (refresher == null || getNumericId == null || instances == null || instances.Length == 0)
                return;
            PerformRefresh(servers, refresher, instances.Select(getNumericId).ToArray());
        }

        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
        {
            if (refresher == null || getGuidId == null || instances == null || instances.Length == 0)
                return;
            PerformRefresh(servers, refresher, instances.Select(getGuidId).ToArray());
        }

        public void PerformRemove<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (refresher == null || getNumericId == null || instances == null || instances.Length == 0)
                return;
            PerformRemove(servers, refresher, instances.Select(getNumericId).ToArray());
        }

        public abstract void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload);
        public abstract void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds);
        public abstract void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds);
        public abstract void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds);
        public abstract void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher);
    }
}
