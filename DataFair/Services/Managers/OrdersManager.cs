using Common;
using DataFair.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DataFair.Services
{
    public class OrdersManager:IHostedService
    {
        private System.Timers.Timer timer = new System.Timers.Timer(Options.OrderGenerationTimerPeriod);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly State state;
        private readonly OrdersGenerator ordersGenerator;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly object sync = new object();

        #region temp generation ruling
        private static bool GenerateGetFullChannelOrders = false;
        private static object sync2 = new object();

        internal static void EnableGetFullChannelOrdersGen()
        {
            lock (sync2)
                GenerateGetFullChannelOrders = true;
        }

        internal static void DisableGetFullChannelOrdersGen()
        {
            lock (sync2)
                GenerateGetFullChannelOrders = false;
        }

        internal static bool GenerateGetFullChannelOrdersStatus()
        {
            lock (sync2)
                return GenerateGetFullChannelOrders;
        }
        #endregion

        public OrdersManager(State state, OrdersGenerator ordersGenerator)
        {
            this.state = state;
            this.ordersGenerator = ordersGenerator;
            timer.Elapsed += TimerAction;
        }

        private void TimerAction(object sender,ElapsedEventArgs args)
        {   
            if (state.Orders.Count==0&&Monitor.TryEnter(sync))
            {
                try
                {
                    Task.WaitAll(ordersGenerator.CreateHistoryLoadingOrders());//, CreateGetFullChannelOrders());
                }
                catch { }
                Monitor.Exit(sync);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Stop();
            cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
