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
using System.Timers;
using Timer = System.Timers.Timer;

namespace DataFair.Services
{
    public class CommonWriter<TData> :ICommonWriter<TData> where TData : class
    {
        private Timer Timer;
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        internal Task WritingTask;
        internal Task FailedWritingTask;
        private readonly ConcurrentQueue<TData> DataQueue = new ConcurrentQueue<TData>();
        private readonly ConcurrentQueue<TData> FailedDataQueue = new ConcurrentQueue<TData>();
        private readonly object sync = new object();
        private readonly IWriterCore<TData> writerSettings;
        private readonly LoadManager loadManager;
        public CommonWriter(IWriterCore<TData> writerSettings, LoadManager loadManager)
        {
            this.writerSettings = writerSettings;
            this.loadManager = loadManager;
            Timer = new Timer();
            Timer.Interval = Options.StartWritingInterval;
            Timer.Elapsed += TryStartWriting;
            Timer.AutoReset = true;
            Timer.Start();
            //Connention = new NpgsqlConnection(writerSettings.ConnectionString);
        }

        private void TryStartWriting(object sender, ElapsedEventArgs args)
        {
            loadManager.AddValue(DataQueue.Count);
            if (Monitor.TryEnter(sync))
            {
                if (DataQueue.Count > 0 && (WritingTask == null || WritingTask.IsCompleted)) 
                {
                    WritingTask = Task.Factory.StartNew(WritingTaskAction);
                }
                if (FailedDataQueue.Count > 0 && (FailedWritingTask == null || FailedWritingTask.IsCompleted)) 
                {
                    FailedWritingTask = Task.Factory.StartNew(FailedWriting);
                }
                Monitor.Exit(sync);
            }
        }
        public void PutData(TData data)
        {
            DataQueue.Enqueue(data);
        }
        public int GetQueueCount()
        {
            return DataQueue.Count + FailedDataQueue.Count;
        }

        public int GetFailedQueueCount()
        {
            return FailedDataQueue.Count;
        }

        private void WritingTaskAction()
        {
            try
            {
                using (DbConnection Connention = new NpgsqlConnection(writerSettings.ConnectionString))
                {
                    Connention.Open();
                    using DbCommand AddMessageCommand = writerSettings.CommandCreator(Connention);
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
                                logger.Error(ex, "Error while writing messages!");
                                transaction.Rollback();
                                Task.Factory.StartNew(AddDataToFail, ReserveDataQueue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
            try
            {
                using (DbConnection Connention = new NpgsqlConnection(writerSettings.ConnectionString))
                {
                    using DbCommand AddMessageCommand = writerSettings.CommandCreator(Connention);
                    while (!FailedDataQueue.IsEmpty&& FailedDataQueue.TryPeek(out TData message))
                    {
                        if (FailedDataQueue.Count > Options.SleepModeStartCount)
                        {
                            FailedDataQueue.Clear();
                            logger.Warn("Clearing failed queue!");
                        }
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
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
