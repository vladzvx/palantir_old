using Common;
using Common.Models;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NLog;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Common.Services
{
    public class CommonWriter<T>: IDisposable, ICommonWriter<T> where T: class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<T> DataQueue = new ConcurrentQueue<T>();
        private readonly IWriterCore<T> writerCore;
        private readonly ConnectionPoolManager manager;
        private readonly IDataBaseSettings settings;
        private readonly CancellationTokenSource globaCts;
        private readonly CancellationTokenSource localCts= new CancellationTokenSource();
        private readonly System.Timers.Timer Timer;
        private readonly object locker = new object();
        private Task WritingTask;
        public CommonWriter(IWriterCore<T> writerCore, IDataBaseSettings settings, ConnectionPoolManager manager, CancellationTokenSource globaCts)
        {
            this.writerCore = writerCore;
            this.manager = manager;
            this.settings = settings;
            this.globaCts = globaCts;
            this.WritingTask = StartWriting(globaCts.Token, localCts.Token);
            Timer = new Timer();
            Timer.Interval = settings.StartWritingInterval;
            Timer.Elapsed += TryStartWriting;
            Timer.AutoReset = true;
            Timer.Start();
        }

        private void TryStartWriting(object sender, ElapsedEventArgs args)
        {
            if (Monitor.TryEnter(locker))
            {
                if (DataQueue.Count > settings.CriticalQueueSize)
                    StartWriting(globaCts.Token, localCts.Token);
                if (DataQueue.Count > 0 && (WritingTask == null || WritingTask.IsCompleted))
                {
                    WritingTask = StartWriting(globaCts.Token, localCts.Token);
                }
                Monitor.Exit(locker);
            }
        }

        public void PutData(T data)
        {
            DataQueue.Enqueue(data);
        }
        public int GetQueueCount()
        {
            return DataQueue.Count;
        }

        private async Task StartWriting(CancellationToken forceStopToken, CancellationToken? softStopToken=null)
        {
            using ConnectionWrapper connectionWrapper = await manager.GetConnection(forceStopToken);
            writerCore.CreateMainCommand(connectionWrapper.Connection);
            while (!forceStopToken.IsCancellationRequested &&
                (softStopToken == null || !((CancellationToken)softStopToken).IsCancellationRequested))
            {
                using (DbTransaction transaction = await connectionWrapper.Connection.BeginTransactionAsync(forceStopToken))
                {
                    try
                    {
                        for (int i = 0; i < settings.TrasactionSize && !forceStopToken.IsCancellationRequested && !DataQueue.IsEmpty; i++)
                        {
                            if (DataQueue.TryDequeue(out T message))
                            {
                                await writerCore.Write(message, forceStopToken);
                            }
                        }
                        await transaction.CommitAsync(forceStopToken);

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error while writing messages!");
                        await transaction.RollbackAsync();
                    }
                }
                await Task.Delay(100);
            }
        }

        public void Dispose()
        {
            localCts.Cancel();
        }

        public async Task ExecuteAdditionalAction(object data)
        {
            using ConnectionWrapper connectionWrapper = await manager.GetConnection(globaCts.Token);
            writerCore.CreateAdditionalCommand(connectionWrapper.Connection);
            await writerCore.AdditionaAcion(data);
        }
    }
}
