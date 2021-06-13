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
    public class CommonWriter<T>: ICommonWriter<T> where T: class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<T> DataQueue = new ConcurrentQueue<T>();
        private readonly IWriterCore<T> writerCore;
        private readonly ConnectionPoolManager manager;
        private readonly IDataBaseSettings settings;
        public CommonWriter(IWriterCore<T> writerCore, IDataBaseSettings settings, ConnectionPoolManager manager)
        {
            this.writerCore = writerCore;
            this.manager = manager;
            this.settings = settings;
        }

        public void PutData(T data)
        {
            DataQueue.Enqueue(data);
        }
        public int GetQueueCount()
        {
            return DataQueue.Count;
        }

        private async Task StartWriting(CancellationToken token)
        {
            using ConnectionWrapper connectionWrapper = await manager.GetConnection(token);
            while (!token.IsCancellationRequested && !DataQueue.IsEmpty)
            {
                using (DbTransaction transaction = await connectionWrapper.Connection.BeginTransactionAsync(token))
                {
                    writerCore.CreateCommand(connectionWrapper.Connection);
                    try
                    {

                        for (int i = 0; i < settings.TrasactionSize && !token.IsCancellationRequested && !DataQueue.IsEmpty; i++)
                        {
                            if (DataQueue.TryDequeue(out T message))
                            {
                                await writerCore.Write(message, token);
                            }
                        }
                        await transaction.CommitAsync(token);

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error while writing messages!");
                        await transaction.RollbackAsync();
                    }
                }
            }
        }
    }
}
