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
    public class ConnectionsFactory
    {
        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private readonly IDataBaseSettings settings;
        public readonly ConcurrentBag<ConnectionWrapper> Pool = new ConcurrentBag<ConnectionWrapper>();
        internal readonly ConcurrentDictionary<Guid, ConnectionWrapper> PoolRepo = new ConcurrentDictionary<Guid, ConnectionWrapper>();
        private readonly object locker = new object();
        public ConnectionsFactory(IDataBaseSettings settings)
        {
            this.settings = settings;
            timer.Interval = settings.PoolingTimeout;
            TimerAction(null, null);
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
                try
                {
                    connection.Connection.Close();
                    connection.Connection.Dispose();
                }
                catch { }
            }

            while (Pool.Count < settings.ConnectionPoolHotReserve && PoolRepo.Count <settings.ConnectionPoolMaxSize)
            {
                try
                {
                    Task<ConnectionWrapper> t = CreateConnectionAsync(CancellationToken.None);
                    Pool.Add(t.Result);
                    PoolRepo.TryAdd(t.Result.Id, t.Result);
                }
                catch { }
            }
        }

        private async Task<ConnectionWrapper> CreateConnectionAsync(CancellationToken token)
        {
            ConnectionWrapper connection = new ConnectionWrapper(settings.ConnectionString, this);
            await connection.Connection.OpenAsync(token);
            PoolRepo.TryAdd(connection.Id, connection);
            return connection;
        }
        public async Task<ConnectionWrapper> GetConnectionAsync(CancellationToken token)
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
                else if (PoolRepo.Count < this.settings.ConnectionPoolMaxSize)
                {
                    if (Monitor.TryEnter(locker))
                    {
                        try
                        {
                            return await CreateConnectionAsync(token);
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            Monitor.Exit(locker);
                        }
                    }
                }
                else await Task.Delay(100, token);
            }
            throw new OperationCanceledException();
        }
        public ConnectionWrapper GetConnection(CancellationToken token)
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
                else if (PoolRepo.Count < this.settings.ConnectionPoolMaxSize)
                {
                    if (Monitor.TryEnter(locker))
                    {
                        try
                        {
                            connection = new ConnectionWrapper(settings.ConnectionString, this);
                            connection.Connection.Open();
                            PoolRepo.TryAdd(connection.Id, connection);
                        }
                        catch (Exception ex)
                        {

                        }
                        Monitor.Exit(locker);
                        return connection;
                    }
                }
                Task.Delay(100, token).Wait();
            }
            return connection;
        }
    }
}
