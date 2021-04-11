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
    internal static class Storage
    {
        internal static Timer timer = new Timer(60000);
        internal static DBWorker worker = new DBWorker(Constants.ConnectionString);


        internal static ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        internal static ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        internal static ConcurrentBag<Common.Collector> Collectors = new ConcurrentBag<Collector>();
        internal static ConcurrentBag<Common.UserInfo> Users = new ConcurrentBag<UserInfo>();
        internal static ConcurrentDictionary<string,Common.UserInfo> AllUsers = new ConcurrentDictionary<string,UserInfo>();
        internal static ConcurrentDictionary<string,Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        internal static ConcurrentDictionary<string,Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();

        private static object sync = new object();
        static Storage()
        {
            LoadCollectorsInfoFromDB(null,null);
            action(null,null);
            timer.Elapsed += action;
            timer.Elapsed += LoadCollectorsInfoFromDB;
            timer.AutoReset = true;
            timer.Start();
        }

        private static void LoadCollectorsInfoFromDB(object sender, ElapsedEventArgs args)
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
                    if (!AllCollectors.ContainsKey(collector.ApiHash))
                    {
                        AllCollectors.TryAdd(collector.ApiHash, collector);
                        Collectors.Add(collector);
                    }
                }
                foreach (UserInfo user in db.UsersInfo.ToList())
                {
                    if (!AllUsers.ContainsKey(user.Phone))
                    {
                        AllUsers.TryAdd(user.Phone, user);
                        Users.Add(user);
                    }
                }
            }
        }
        private static void action(object sender, ElapsedEventArgs args)
        {
            if (System.Threading.Monitor.TryEnter(sync))
            {
                worker.CreateTasksByUnupdatedChats(DateTime.Now.AddMinutes(-20));
                System.Threading.Monitor.Exit(sync);
            }
        }


        internal static StateReport GetState()
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
                Entities = Storage.worker.GetEntitiesNumberInQueue(),
                Messages = Storage.worker.GetMessagesNumberInQueue(),
                Collectors = Storage.Collectors.Count,
                Users = Storage.Users.Count,
                SessionsStorages = Storage.SessionStorages.Count,
                Orders = Storage.Orders.Count,
                MemoryUsed = Memory,
                FreeDisk = Disk
            };
        }
    }
}
