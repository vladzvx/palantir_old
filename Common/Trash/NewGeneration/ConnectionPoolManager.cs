using Common.Services.DataBase.Interfaces;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class ConnectionPoolManager
    {
        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private readonly IDataBaseSettings settings;
        public readonly ConcurrentBag<ConnectionWrapper> Pool = new ConcurrentBag<ConnectionWrapper>();
        private readonly ConcurrentDictionary<int, ConnectionWrapper> PoolRepo = new ConcurrentDictionary<int, ConnectionWrapper>();
        private readonly object locker = new object();
        public ConnectionPoolManager(IDataBaseSettings settings)
        {
            this.settings = settings;
            timer.Interval = settings.PoolingTimeout;
            timer.Elapsed += TimerAction;
            timer.AutoReset = true;
            timer.Start();
        }

        private void TimerAction(object sender, System.Timers.ElapsedEventArgs args)
        {
            while (Pool.Count > settings.ConnectionPoolHotReserve &&
                Pool.TryTake(out ConnectionWrapper connection) &&
                PoolRepo.TryRemove(connection.Id, out _))
            {
                connection.Connection.Close();
                connection.Connection.Dispose();
            }
        }

        public async Task<ConnectionWrapper> GetConnection(CancellationToken token)
        {
            ConnectionWrapper connection = null;
            while (!token.IsCancellationRequested)
            {
                if (Pool.TryTake(out connection))
                {
                    return connection;
                }
                else if (PoolRepo.Count < this.settings.ConnectionPoolMaxSize && Monitor.TryEnter(locker))
                {
                    try
                    {
                        connection = new ConnectionWrapper(settings.ConnectionString, PoolRepo.Keys.Count>0? PoolRepo.Keys.Max()+1:0, this);
                        await connection.Connection.OpenAsync();
                        PoolRepo.TryAdd(connection.Id, connection);
                        return connection;
                    }
                    catch (Exception ex)
                    {

                    }
                    Monitor.Exit(locker);
                }
                else await Task.Delay(100, token);
            }
            return connection;
        }
    }
}
