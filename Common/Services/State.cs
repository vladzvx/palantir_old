using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Services
{
    public class State
    {
        public ConcurrentQueue<Report> Reports = new ConcurrentQueue<Report>();
        public ConcurrentQueue<Order> MaxPriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> MiddlePriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        public ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        public ConcurrentDictionary<string, ConcurrentBag<Common.Collector>> Collectors = new ConcurrentDictionary<string, ConcurrentBag<Collector>>();
        public ConcurrentDictionary<string, Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        public ConcurrentDictionary<string, Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();
    }
}
