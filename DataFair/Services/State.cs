using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services
{
    public class State
    {
        internal ConcurrentQueue<Order> MaxPriorityOrders = new ConcurrentQueue<Order>();
        internal ConcurrentQueue<Order> MiddlePriorityOrders = new ConcurrentQueue<Order>();
        internal ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        internal ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        internal ConcurrentBag<Common.Collector> Collectors = new ConcurrentBag<Collector>();
        internal ConcurrentDictionary<string, Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        internal ConcurrentDictionary<string, Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();
    }
}
