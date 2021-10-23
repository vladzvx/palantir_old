using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class ConnectionsFactory
    {
        //private readonly System.Timers.Timer timer = new System.Timers.Timer();
        public readonly IDataBaseSettings settings;
        public readonly ConcurrentQueue<ConnectionWrapper> CancellationQueue = new ConcurrentQueue<ConnectionWrapper>();
        public readonly ConcurrentQueue<ConnectionWrapper> Pool = new ConcurrentQueue<ConnectionWrapper>();
        internal readonly ConcurrentDictionary<DateTime, ConnectionWrapper> PoolRepo = new ConcurrentDictionary<DateTime, ConnectionWrapper>();
        private readonly object locker = new object();
        private readonly Thread worker;
        private readonly Thread Canceller;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public int TotalConnections
        {
            get { return PoolRepo.Count; }
        }
        public int HotReserve
        {
            get { return Pool.Count; }
        }
        public ConnectionsFactory(IDataBaseSettings settings)
        {
            this.settings = settings;
            settings.Token = cts.Token;
            worker = new Thread(new ParameterizedThreadStart(ThreadAction));
            Canceller = new Thread(new ParameterizedThreadStart(CancellationThreadAction));
            worker.Start(settings);
            Canceller.Start(settings);

            //timer.Interval = settings.PoolingTimeout;
            //TimerAction(null, null);
            //timer.Elapsed += TimerAction;
            //timer.AutoReset = true;
            //timer.Start();
        }

        ~ConnectionsFactory()
        {
            cts.Cancel();
        }
        private void ThreadAction(object cancellationToken)
        {
            if (cancellationToken is IDataBaseSettings settings)
            {
                while (!settings.Token.IsCancellationRequested)
                {

                    while (Pool.Count > settings.ConnectionPoolHotReserve &&
                        Pool.TryDequeue(out ConnectionWrapper connection) &&
                        PoolRepo.TryRemove(connection.CreationTime, out _))
                    {
                        try
                        {
                            connection.Connection.Close();
                            connection.Connection.Dispose();
                        }
                        catch { }
                    }

                    while (Pool.Count < settings.ConnectionPoolHotReserve && PoolRepo.Count < settings.ConnectionPoolMaxSize)
                    {
                        try
                        {
                            CreateConnection();
                            //Pool.Add(cw);
                            //PoolRepo.TryAdd(cw.CreationTime, cw);
                        }
                        catch { }
                    }
                    Thread.Sleep(settings.PoolingTimeout);
                }
            }
        }

        private void CancellationThreadAction(object cancellationToken)
        {
            if (cancellationToken is IDataBaseSettings settings)
            {
                while (!settings.Token.IsCancellationRequested)
                {
                    while(CancellationQueue.TryDequeue(out var wrapper))
                    {
                        wrapper.Connection.Close();
                        wrapper.Connection.Dispose();
                        PoolRepo.TryRemove(wrapper.CreationTime, out _);
                    }
                    Thread.Sleep(settings.PoolingTimeout);
                }
            }
        }
        private async Task<ConnectionWrapper> CreateConnectionAsync(CancellationToken token)
        {
            ConnectionWrapper connection = new ConnectionWrapper(settings.ConnectionString1, this);
            await connection.Connection.OpenAsync(token);
            PoolRepo.TryAdd(connection.CreationTime, connection);
            return connection;
        }

        private ConnectionWrapper CreateConnection()
        {
            ConnectionWrapper connection = new ConnectionWrapper(settings.ConnectionString1, this);
            connection.Connection.Open();
            while (PoolRepo.ContainsKey(connection.CreationTime))
            {
                connection.CreationTime = DateTime.UtcNow;
            }
            Pool.Enqueue(connection);
            PoolRepo.TryAdd(connection.CreationTime, connection);
            return connection;
        }

        public async Task<ConnectionWrapper> GetConnectionAsync(CancellationToken token)
        {
            ConnectionWrapper connection = null;
            while (!token.IsCancellationRequested)
            {
                if (Pool.TryDequeue(out connection))
                {
                    if (connection.Connection.FullState == System.Data.ConnectionState.Open)
                    {
                        return connection;
                    }
                    else if (connection.Connection.FullState == System.Data.ConnectionState.Connecting)
                    {
                        continue;
                    }
                    else
                    {
                        PoolRepo.TryRemove(connection.CreationTime, out var _);
                    }
                }
                else if (PoolRepo.Count < this.settings.ConnectionPoolMaxSize)
                {
                    //if (Monitor.TryEnter(locker))
                    {
                        try
                        {
                            connection = await CreateConnectionAsync(token);
                            //Monitor.Exit(locker);
                            return connection;
                        }
                        catch (Exception)
                        {
                            // Monitor.Exit(locker);
                        }
                    }
                }
                else
                {
                    await Task.Delay(100, token);
                }
            }
            throw new OperationCanceledException();
        }
    }
}
