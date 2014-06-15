using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using StackExchange.Profiling.Data;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Sync.QueryNotification.Extensions
{
    public static class DatabaseExtensions
    {
        public static IEnumerable<T> QueryWithNotification<T>(this Database database, Action<SqlNotificationEventArgs> notification, int timeout, Sql sql)
        {
            return QueryWithNotification<T>(database, notification, timeout, sql.SQL, sql.Arguments);
        }

        public static IEnumerable<T> QueryWithNotification<T>(this Database database, Action<SqlNotificationEventArgs> notification, int timeout, string sql, params object[] args)
        {
            database.OpenSharedConnection();
            try
            {
                using (var cmd = database.CreateCommand(database.Connection, sql, args))
                {
                    var profiledCommand = cmd as ProfiledDbCommand;
                    if (profiledCommand == null)
                        throw new NotSupportedException(string.Format("Query Notification Requires {0}", typeof(ProfiledDbCommand).FullName));

                    var sqlCommand = profiledCommand.InternalCommand as SqlCommand;
                    if (sqlCommand == null)
                        throw new NotSupportedException(string.Format("Query Notification Requires {0}", typeof(SqlCommand).FullName));

                    var dependency = new SqlDependency(sqlCommand, null, timeout);
                    dependency.OnChange += (sender, eventArgs) => notification(eventArgs);

                    IDataReader r;
                    var pd = Database.PocoData.ForType(typeof(T));
                    try
                    {
                        r = cmd.ExecuteReader();
                        database.OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        database.OnException(x);
                        throw;
                    }
                    var factory = pd.GetFactory(cmd.CommandText, database.Connection.ConnectionString, database.ForceDateTimesToUtc, 0, r.FieldCount, r) as Func<IDataReader, T>;
                    using (r)
                    {
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read())
                                    yield break;
                                poco = factory(r);
                            }
                            catch (Exception x)
                            {
                                database.OnException(x);
                                throw;
                            }

                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                database.CloseSharedConnection();
            }
        }
    }
}
