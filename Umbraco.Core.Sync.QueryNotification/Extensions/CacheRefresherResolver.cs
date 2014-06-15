using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using umbraco.interfaces;

namespace Umbraco.Core.Sync.QueryNotification.Extensions
{
    public static class CacheRefresherResolver
    {
        public static IList<ICacheRefresher> GetRefreshers()
        {
            //Tricky As Singleton CacheRefreshersResolver Is Marked As Internal In The Umbraco Core
            var type = Type.GetType("Umbraco.Core.CacheRefreshersResolver, Umbraco.Core");
            if (type == null)
                throw new Exception("Umbraco.Core.CacheRefreshersResolver, Umbraco.Core Could Not Be Found");
            var current = type.GetProperty("Current", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var refreshers = type.GetProperty("CacheRefreshers", BindingFlags.Public | BindingFlags.Instance);
            var instance = current.GetValue(null);
            return ((IEnumerable<ICacheRefresher>) refreshers.GetValue(instance)).ToList();
        }
    }
}
