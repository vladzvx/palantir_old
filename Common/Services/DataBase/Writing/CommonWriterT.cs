using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Common.Services
{
    public class CommonWriter<T>: ICommonWriter<T> where T: class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<T> DataQueue = new ConcurrentQueue<T>();
        private readonly IWriterCore<T> writerCore;
        private readonly ConnectionsFactory manager;
        private readonly IDataBaseSettings settings;
        private readonly CancellationTokenSource globaCts;
        private readonly System.Timers.Timer Timer;
        private readonly object locker = new object();
        private Task WritingTask;
        public CommonWriter(IWriterCore<T> writerCore, IDataBaseSettings settings, ConnectionsFactory manager, CancellationTokenSource globaCts)
        {
            this.writerCore = writerCore;
            this.manager = manager;
            this.settings = settings;
            this.globaCts = globaCts;
            this.WritingTask = Task.Factory.StartNew(WritingAction, TaskCreationOptions.LongRunning);

            Timer = new Timer();
            Timer.Interval = settings.StartWritingInterval;
            Timer.Elapsed += ManageWriting;
            Timer.AutoReset = true;
            Timer.Start();
        }
        private void ManageWriting(object sender, ElapsedEventArgs args)
        {
            if (Monitor.TryEnter(locker))
            {
                if (DataQueue.Count > settings.CriticalQueueSize)
                {
                    Task.Factory.StartNew(WritingAction, globaCts.Token, TaskCreationOptions.LongRunning);
                }

                if (DataQueue.Count > 0 && (WritingTask == null || WritingTask.IsCompleted))
                {
                    WritingTask = Task.Factory.StartNew(WritingAction, globaCts.Token, TaskCreationOptions.LongRunning);
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
        private async Task WritingAction(object CancellationToken)
        {
            if (CancellationToken is not CancellationToken forceStopToken) return;

            using (ConnectionWrapper connectionWrapper = await manager.GetConnectionAsync(forceStopToken))
            {
                DbCommand command = writerCore.CreateMainCommand(connectionWrapper.Connection);
                while (!forceStopToken.IsCancellationRequested && !DataQueue.IsEmpty)
                {
                    using (DbTransaction transaction = await connectionWrapper.Connection.BeginTransactionAsync(forceStopToken))
                    {
                        try
                        {
                            for (int i = 0; i < settings.TrasactionSize && !forceStopToken.IsCancellationRequested && !DataQueue.IsEmpty; i++)
                            {
                                if (DataQueue.TryDequeue(out T message))
                                {
                                    await writerCore.ExecuteWriting(command, message, forceStopToken);
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
            
        }
        public async Task ExecuteAdditionalAction(object data)
        {
            try
            {
                using (ConnectionWrapper connectionWrapper = await manager.GetConnectionAsync(globaCts.Token))
                {
                    DbCommand command = writerCore.CreateAdditionalCommand(connectionWrapper.Connection);
                    await writerCore.ExecuteAdditionaAcion(command, data, globaCts.Token);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while executing AdditionalAction!");
            }
        }
    }
}