using Common;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Common.Services
{
    public class OrdersManager : IHostedService
    {
        private System.Timers.Timer timer = new System.Timers.Timer(Options.OrderGenerationTimerPeriod);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly State state;
        private readonly OrdersGenerator ordersGenerator;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly object sync = new object();
        private bool GenerationOn = true;
        public OrdersManager(State state, OrdersGenerator ordersGenerator)
        {
            this.state = state;
            this.ordersGenerator = ordersGenerator;
            timer.Elapsed += TimerAction;
        }

        private void TimerAction(object sender, ElapsedEventArgs args)
        {
            if (Monitor.TryEnter(sync))
            {
                try
                {
                
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
