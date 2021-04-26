using Common;
using DataFair.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DataFair.Services
{
    public class CollectorsManager : IHostedService
    {
        internal Timer timer;
        private readonly State state;

        public CollectorsManager(State state)
        {
            timer = new Timer(Options.CollectorsSyncTimerPeriod);
            this.state = state;
            LoadCollectorsInfo(null,null);
            timer.Elapsed += LoadCollectorsInfo;
            timer.AutoReset = true;
        }

        private void LoadCollectorsInfo(object sender, ElapsedEventArgs args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (SessionSettings session in db.Sessions.ToList())
                {
                    if (!state.AllSessions.ContainsKey(session.SessionStorageHost))
                    {
                        state.AllSessions.TryAdd(session.SessionStorageHost, session);
                        state.SessionStorages.Add(session);
                    }
                }
                foreach (Collector collector in db.Collectors.ToList())
                {
                    if (!state.AllCollectors.ContainsKey(collector.Phone))
                    {
                        state.AllCollectors.TryAdd(collector.Phone, collector);
                        state.Collectors.Add(collector);
                    }
                }
            }
        }
        public Task StartAsync(System.Threading.CancellationToken cancellationToken)
        {
            timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(System.Threading.CancellationToken cancellationToken)
        {
            timer.Stop();
            return Task.CompletedTask;
        }
    }
}
