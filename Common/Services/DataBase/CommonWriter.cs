using Common;
using Common.Models;
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
    public class CommonWriter:ICommonWriter 
    {
        private Timer Timer;
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        internal Task WritingTask;
        private readonly ConcurrentQueue<object> DataQueue = new ConcurrentQueue<object>();
        private readonly object sync = new object();
        private readonly IWriterCore writerSettings;
        private readonly LoadManager loadManager;
        private DbConnection Connention;
        public CommonWriter(LoadManager loadManager, IWriterCore writerSettings)
        {
            this.writerSettings = writerSettings;
            this.loadManager = loadManager;
            Timer = new Timer();
            Timer.Interval = writerSettings.StartWritingInterval;
            Timer.Elapsed += TryStartWriting;
            Timer.AutoReset = true;
            Timer.Start();
            Connention = new NpgsqlConnection(writerSettings.ConnectionString);
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
                Monitor.Exit(sync);
            }
        }
        public void PutData(object data)
        {
            DataQueue.Enqueue(data);
        }
        public int GetQueueCount()
        {
            return DataQueue.Count;
        }

        private bool ManageConnection()
        {
            int TryCounter = 0;
            while (Connention.State != System.Data.ConnectionState.Open && TryCounter < Options.ReconnectionRepeatCount)
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
                        Thread.Sleep(writerSettings.ReconnerctionPause);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while reconnection");
                }
            }
            return Connention.State == System.Data.ConnectionState.Open;
        }

        private void WritingTaskAction()
        {
            try
            {
                if (ManageConnection())
                {
                    using DbCommand AddMessageCommand = writerSettings.CreateCommand(Connention,typeof(Message));
                    using DbCommand AddChatCommand = writerSettings.CreateCommand(Connention,typeof(Chat));
                    using DbCommand AddUserCommand = writerSettings.CreateCommand(Connention, typeof(User));
                    using DbCommand BanChatCommand = writerSettings.CreateCommand(Connention, typeof(Ban));
                    using DbCommand DelMessageCommand = writerSettings.CreateCommand(Connention, typeof(Deleting));
                    {
                        using (DbTransaction transaction = Connention.BeginTransaction())
                        {
                            try
                            {
                                for (int i = 0; i < Options.WriterTransactionSize && !DataQueue.IsEmpty; i++)
                                {
                                    if (DataQueue.TryDequeue(out object message))
                                    {
                                        writerSettings.WriteSingleObject(message, transaction);
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Error while writing messages!");
                                transaction.Rollback();
                                throw ex;
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
    }
}
