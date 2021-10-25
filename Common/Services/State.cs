using Common.Services.DataBase.Interfaces;
using System.Collections.Concurrent;

namespace Common.Services
{
    public class CounterWrapper
    {
        public int count = 1;
    }
    public class State
    {
        public State(ILoadManager loadManager, ILimits limits)
        {
            this.loadManager = loadManager;
            this.limits = limits;
        }
        public OrdersManager ordersManager;
        private readonly ILoadManager loadManager;
        private readonly ILimits limits;
        private readonly Order empty = new Order() { Type = OrderType.Empty, status = Order.Status.Executed };
        private readonly Order sleep = new Order() { Type = OrderType.Sleep, status = Order.Status.Executed, Time = 60 };
        public ConcurrentQueue<Report> Reports = new ConcurrentQueue<Report>();
        public ConcurrentQueue<Order> MaxPriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> MiddlePriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        public ConcurrentDictionary<string, ConcurrentQueue<Order>> TargetedOrders = new ConcurrentDictionary<string, ConcurrentQueue<Order>>();
        public ConcurrentDictionary<string, int> HeavyOrdersCount = new ConcurrentDictionary<string, int>();
        public ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        public ConcurrentDictionary<string, ConcurrentBag<Common.Collector>> Collectors = new ConcurrentDictionary<string, ConcurrentBag<Collector>>();
        public ConcurrentDictionary<string, Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        public ConcurrentDictionary<string, Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();

        public ConcurrentDictionary<string, CounterWrapper> ExecutingOrdersJournal = new ConcurrentDictionary<string, CounterWrapper>();
        public void ClearOrders()
        {
            Orders.Clear();
            MiddlePriorityOrders.Clear();
            MaxPriorityOrders.Clear();
            TargetedOrders.Clear();
        }

        public void AddOrder(Order order)
        {
            if (order.status != Order.Status.Created)
            {
                return;
            }

            if (order.Finders.Count > 0)
            {
                foreach (string finder in order.Finders)
                {
                    if (TargetedOrders.ContainsKey(finder))
                    {
                        TargetedOrders[finder].Enqueue(order);
                    }
                    else
                    {
                        TargetedOrders.TryAdd(finder, new ConcurrentQueue<Order>());
                        TargetedOrders[finder].Enqueue(order);
                    }
                }
            }
            else
            {
                Orders.Enqueue(order);
            }
        }

        public int CountTargetOrders()
        {
            int result = 0;
            foreach (string finder in TargetedOrders.Keys)
            {
                if (TargetedOrders.TryGetValue(finder, out var que))
                {
                    result += que.Count;
                }
            }
            return result;
        }
        public int CountOrders()
        {
            int result = 0;
            result += Orders.Count;
            result += MaxPriorityOrders.Count;
            result += MiddlePriorityOrders.Count;
            return result;

        }

        private void TryIncrementCounter(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (ExecutingOrdersJournal.TryGetValue(key, out var val))
                {
                    val.count++;
                }
                else
                {
                    ExecutingOrdersJournal.TryAdd(key, new CounterWrapper());
                }
            }
        }

        private bool CheckCounter(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (ExecutingOrdersJournal.TryGetValue(key, out var val))
                {
                    bool temp = val.count < limits.MaxOrdersNumber;
                    return temp;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetOrder(OrderRequest req, out Order order)
        {
            if (loadManager.CheckPauseNecessity())
            {
                order = sleep;
                return true;
            }
            order = empty;

            if (!req.Banned)
            {
                if (!CheckCounter(req.Finder))
                {
                    order = sleep;
                    return true;
                }

                while (MaxPriorityOrders.TryDequeue(out Order temp))
                {
                    if (temp.TryGet())
                    {
                        TryIncrementCounter(req.Finder);
                        order = temp;
                        return true;
                    }
                }
                while (MiddlePriorityOrders.TryDequeue(out Order temp))
                {
                    if (temp.TryGet())
                    {
                        TryIncrementCounter(req.Finder);
                        order = temp;
                        return true;
                    }
                }
                while (Orders.TryDequeue(out Order temp))
                {
                    if (temp.TryGet())
                    {
                        TryIncrementCounter(req.Finder);
                        order = temp;
                        return true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(req.Finder))
            {
                while (TargetedOrders.ContainsKey(req.Finder) && TargetedOrders[req.Finder].TryDequeue(out order))
                {
                    if (order.Type == OrderType.Pair || order.Type == OrderType.GetFullChannel)
                    {
                        if (!CheckCounter(req.Finder))
                        {
                            continue;
                        }
                        else
                        {
                            if (order.TryGet())
                            {
                                TryIncrementCounter(req.Finder);
                                //TargetedOrders[req.Finder].Enqueue(order);
                                return true;
                            }
                        }
                    }
                    else if (order.Type==OrderType.History && order.repeatInterval!=null)
                    {
                        if (order.TryGet())
                        {
                            TargetedOrders[req.Finder].Enqueue(order);
                            return true;
                        }
                    }
                    else
                    {
                        if (order.TryGet())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
