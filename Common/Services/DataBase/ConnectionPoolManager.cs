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
        internal readonly ConcurrentDictionary<Guid, ConnectionWrapper> PoolRepo = new ConcurrentDictionary<Guid, ConnectionWrapper>();
        private readonly object locker = new object();
        public ConnectionPoolManager(IDataBaseSettings settings)
        {
            this.settings = settings;
            timer.Interval = settings.PoolingTimeout;
            timer.Elapsed += TimerAction;
            timer.AutoReset = true;
            timer.Start();

            //ConnectionWrapper connection = new ConnectionWrapper(settings.ConnectionString, this);
            //connection.Connection.Open();
            //PoolRepo.TryAdd(connection.Id, connection);
            //Pool.Add(connection);

            //ConnectionWrapper connection2 = new ConnectionWrapper(settings.ConnectionString, this);
            //connection2.Connection.Open();
            //PoolRepo.TryAdd(connection2.Id, connection2);
            //Pool.Add(connection2);
        }

        private void TimerAction(object sender, System.Timers.ElapsedEventArgs args)
        {
            while (Pool.Count > settings.ConnectionPoolHotReserve &&
                Pool.TryTake(out ConnectionWrapper connection) &&
                PoolRepo.TryRemove(connection.Id, out _))
            {
                try
                {
                    connection.Connection.Close();
                    connection.Connection.Dispose();
                }
                catch { }

            }
        }

        public async Task<ConnectionWrapper> GetConnection(CancellationToken token)
        {
            ConnectionWrapper connection = null;
            while (!token.IsCancellationRequested)
            {
                if (Pool.TryTake(out connection))
                {
                    if (connection.Connection.FullState == System.Data.ConnectionState.Open)
                        return connection;
                    else if (connection.Connection.FullState == System.Data.ConnectionState.Connecting)
                    {
                        continue;
                    }
                    else
                    {
                        PoolRepo.TryRemove(connection.Id, out var _);
                    }
                }
                else if (PoolRepo.Count < this.settings.ConnectionPoolMaxSize && Monitor.TryEnter(locker))
                {
                    try
                    {
                        connection = new ConnectionWrapper(settings.ConnectionString, this);
                        await connection.Connection.OpenAsync();
                        PoolRepo.TryAdd(connection.Id, connection);
                        return connection;
                    }
                    catch (Exception ex)
                    {

                    }
                    Monitor.Exit(locker);
                }
                await Task.Delay(100, token);
            }
            return connection;
        }
    }
}
