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
        private readonly Order empty = new Order() { Type = OrderType.Empty, status=Order.Status.Executed };
        public ConcurrentQueue<Report> Reports = new ConcurrentQueue<Report>();
        public ConcurrentQueue<Order> MaxPriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> MiddlePriorityOrders = new ConcurrentQueue<Order>();
        public ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        public ConcurrentDictionary<string, ConcurrentQueue<Order>> TargetedOrders = new ConcurrentDictionary<string, ConcurrentQueue<Order>>();
        public ConcurrentBag<SessionSettings> SessionStorages = new ConcurrentBag<SessionSettings>();
        public ConcurrentDictionary<string, ConcurrentBag<Common.Collector>> Collectors = new ConcurrentDictionary<string, ConcurrentBag<Collector>>();
        public ConcurrentDictionary<string, Common.SessionSettings> AllSessions = new ConcurrentDictionary<string, SessionSettings>();
        public ConcurrentDictionary<string, Common.Collector> AllCollectors = new ConcurrentDictionary<string, Collector>();

        public void ClearOrders()
        {
            Orders.Clear();
            MiddlePriorityOrders.Clear();
            MaxPriorityOrders.Clear();
            TargetedOrders.Clear();
        }

        public void AddOrder(Order order)
        {
            if (order.status != Order.Status.Created) return;
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

        public bool TryGetOrder(string finder, out Order order)
        {
            order = empty;
            if (string.IsNullOrEmpty(finder))
            {
                while (Orders.TryDequeue(out Order temp))
                {
                    if (temp.status == Order.Status.Created)
                    {
                        order = temp;
                        temp.status = Order.Status.Getted;
                        return true;
                    }
                }
            }
            else
            {
                while (TargetedOrders.ContainsKey(finder) && TargetedOrders[finder].TryDequeue(out order))
                {
                    if (order.status == Order.Status.Created)
                    {
                        order.status = Order.Status.Getted;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
