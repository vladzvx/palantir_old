using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DataFair
{
    internal static class Storage
    {
        internal static Timer timer = new Timer(20000);
        internal static DBWorker worker = new DBWorker(Constants.ConnectionString);


        internal static ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        internal static ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        internal static ConcurrentBag<Common.Collector> Collectors = new ConcurrentBag<Collector>();
        internal static ConcurrentBag<Common.UserInfo> Users = new ConcurrentBag<UserInfo>();

        private static object sync = new object();
        static Storage()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (SessionSettings session in db.Sessions.ToList())
                {
                    SessionStorages.Add(session);
                }
                foreach (Collector collector in db.Collectors.ToList())
                {
                    Collectors.Add(collector);
                }
                foreach (UserInfo user in db.UsersInfo.ToList())
                {
                    Users.Add(user);
                }
            }

            timer.Elapsed += action;
            timer.AutoReset = true;
            timer.Start();
        }

        private static void action(object sender, ElapsedEventArgs args)
        {
            if (System.Threading.Monitor.TryEnter(sync))
            {
                worker.CreateTasksByUnupdatedChats(DateTime.UtcNow.AddHours(-24));
                System.Threading.Monitor.Exit(sync);
            }
        }
    }
}
