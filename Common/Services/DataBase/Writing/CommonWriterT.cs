using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class CommonWriter<T> : ActionPeriodicExecutor, ICommonWriter<T> where T : class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<T> DataQueue = new ConcurrentQueue<T>();
        private readonly IWriterCore<T> writerCore;
        private readonly ConnectionsFactory manager;
        private readonly IDataBaseSettings settings;

        public CommonWriter(IWriterCore<T> writerCore, IDataBaseSettings settings, ConnectionsFactory manager, CancellationTokenSource globaCts) :
            base(settings.StartWritingInterval, globaCts)
        {
            this.writerCore = writerCore;
            this.manager = manager;
            this.settings = settings;
            this.SetAction(WritingActionWrapper);
            Start();
        }

        public void PutData(T data)
        {
            DataQueue.Enqueue(data);
        }
        public int GetQueueCount()
        {
            return DataQueue.Count;
        }

        private void WritingActionWrapper(object CancellationToken)
        {
            if (CancellationToken is not CancellationToken forceStopToken)
            {
                return;
            }

            WritingAction(forceStopToken).Wait();
        }
        private async Task WritingAction(CancellationToken forceStopToken)
        {
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
                using (ConnectionWrapper connectionWrapper = await manager.GetConnectionAsync(cancellationTokenSource.Token))
                {
                    await writerCore.ExecuteAdditionaAcion(connectionWrapper.Connection, data, cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while executing AdditionalAction!");
            }
        }
    }
}