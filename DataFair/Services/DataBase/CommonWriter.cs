using DataFair.Services.Interfaces;
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

namespace DataFair.Services
{
    public class CommonWriter<TData> :ICommonWriter<TData> where TData : class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        internal Task WritingTask;
        internal Task FailedWritingTask;
        private readonly ConcurrentQueue<TData> DataQueue = new ConcurrentQueue<TData>();
        private readonly ConcurrentQueue<TData> FailedDataQueue = new ConcurrentQueue<TData>();
        private readonly object sync = new object();
        private readonly IWriterCore<TData> writerSettings;
        private DbConnection Connention;
        public CommonWriter(IWriterCore<TData> writerSettings)
        {
            this.writerSettings = writerSettings;
            Connention = new NpgsqlConnection(writerSettings.ConnectionString);
        }

        public void PutData(TData data)
        {
            DataQueue.Enqueue(data);
            if (Monitor.TryEnter(sync))
            {
                if (WritingTask == null || WritingTask.IsCompleted)
                {
                    WritingTask = Task.Factory.StartNew(WritingTaskAction);
                }
                if (FailedWritingTask == null || FailedWritingTask.IsCompleted)
                {
                    FailedWritingTask = Task.Factory.StartNew(FailedWriting);
                }
                Monitor.Exit(sync);
            }
        }

        public int GetQueueCount()
        {
            return DataQueue.Count+ FailedDataQueue.Count;
        }

        private bool ManageConnection()
        {
            int TryCounter = 0;
            while (Connention.State != System.Data.ConnectionState.Open && TryCounter<Options.ReconnectionRepeatCount)
            {
                try
                {
                    if (Connention.State == System.Data.ConnectionState.Closed)
                    {
                        Connention.Open();
                    }
                    else if (Connention.State == System.Data.ConnectionState.Broken)
                    {
                        Connention.Close();
                        Connention.Open();
                    }
                    else
                    {
                        Thread.Sleep(Options.ReconnerctionPause);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex,"Error while reconnection");
                }
            }
            return Connention.State == System.Data.ConnectionState.Open;
        }
        private void WritingTaskAction()
        {
            if (ManageConnection())
            {
                using DbCommand AddMessageCommand = writerSettings.CommandCreator(Connention);
                try
                {
                    while (!DataQueue.IsEmpty)
                    {
                        List<TData> ReserveDataQueue = new List<TData>();
                        using (DbTransaction transaction = Connention.BeginTransaction())
                        {
                            try
                            {
                                for (int i = 0; i < writerSettings.TrasactionSize && !DataQueue.IsEmpty; i++)
                                {
                                    if (DataQueue.TryDequeue(out TData message))
                                    {
                                        ReserveDataQueue.Add(message);
                                        AddMessageCommand.Transaction = transaction;
                                        writerSettings.WriteSingleObject(AddMessageCommand, message);
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Task.Factory.StartNew(AddDataToFail, ReserveDataQueue);
                                logger.Error(ex, "Error while writing messages!");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        private void AddDataToFail(object? data)
        {
            List<TData> datas = (List<TData>)data;
            if (datas != null)
            {
                foreach (TData d in datas)
                {
                    FailedDataQueue.Enqueue(d);
                }
            }
        }
        private void FailedWriting()
        {
            using (DbConnection Connention = new NpgsqlConnection(writerSettings.ConnectionString))
            {
                using DbCommand AddMessageCommand = writerSettings.CommandCreator(Connention);
                try
                {
                    while (!FailedDataQueue.IsEmpty&& FailedDataQueue.TryPeek(out TData message))
                    {
                        try
                        {
                            while (FailedDataQueue.TryDequeue(out message))
                            {
                                writerSettings.WriteSingleObject(AddMessageCommand, message);
                            }
                        } 
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error while writing FailedMessage! Data: "+ Newtonsoft.Json.JsonConvert.SerializeObject(message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

        }
    }
}
