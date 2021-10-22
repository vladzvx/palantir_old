using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class ConnectionsFactory
    {
        //private readonly System.Timers.Timer timer = new System.Timers.Timer();
        public readonly IDataBaseSettings settings;
        public readonly ConcurrentBag<ConnectionWrapper> Pool = new ConcurrentBag<ConnectionWrapper>();
        internal readonly ConcurrentDictionary<DateTime, ConnectionWrapper> PoolRepo = new ConcurrentDictionary<DateTime, ConnectionWrapper>();
        private readonly object locker = new object();
        private readonly Thread worker;
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
            worker.Start(settings);

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
                        Pool.TryTake(out ConnectionWrapper connection) &&
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

        private void TimerAction(object sender, System.Timers.ElapsedEventArgs args)
        {
            while (Pool.Count > settings.ConnectionPoolHotReserve &&
                Pool.TryTake(out ConnectionWrapper connection) &&
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
                    Task<ConnectionWrapper> t = CreateConnectionAsync(CancellationToken.None);
                    Pool.Add(t.Result);
                    PoolRepo.TryAdd(t.Result.CreationTime, t.Result);
                }
                catch { }
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
            Pool.Add(connection);
            PoolRepo.TryAdd(connection.CreationTime, connection);
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
