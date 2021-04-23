using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DataFair
{
    public class State
    {
        internal Timer timer = new Timer(20000);
        internal WorkDB worker = new WorkDB(Options.ConnectionString);
        internal ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        internal ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        internal ConcurrentBag<Common.Collector> Collectors = new ConcurrentBag<Collector>();
        internal ConcurrentDictionary<string,Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        internal ConcurrentDictionary<string,Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();

        public State()
        {
            LoadCollectorsInfo(null,null);
            timer.Elapsed += LoadCollectorsInfo;
            timer.AutoReset = true;
            timer.Start();
        }
        private void LoadCollectorsInfo(object sender, ElapsedEventArgs args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (SessionSettings session in db.Sessions.ToList())
                {
                    if (!AllSessions.ContainsKey(session.SessionStorageHost))
                    {
                        AllSessions.TryAdd(session.SessionStorageHost, session);
                        SessionStorages.Add(session);
                    }
                }
                foreach (Collector collector in db.Collectors.ToList())
                {
                    if (!AllCollectors.ContainsKey(collector.Phone))
                    {
                        AllCollectors.TryAdd(collector.Phone, collector);
                        Collectors.Add(collector);
                    }
                }
            }
        }
        internal StateReport GetStateReport()
        {
            long Memory = 0;
            long Disk = 0;
            foreach (Process proc in Process.GetProcesses())
            {
                Memory += proc.WorkingSet64;
            }
            Memory = Memory / 1024 / 1024;

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    Disk += d.TotalFreeSpace;
                }
            }
            Disk = Disk / 1024 / 1024 / 1024;
            return new StateReport()
            {
                Entities = worker.GetEntitiesNumberInQueue(),
                Messages = worker.GetMessagesNumberInQueue(),
                Collectors = Collectors.Count,
                SessionsStorages = SessionStorages.Count,
                Orders = Orders.Count,
                MemoryUsed = Memory,
                FreeDisk = Disk
            };
        }
    }
}
