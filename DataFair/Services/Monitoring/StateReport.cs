using Common;
using DataFair.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services
{
    public class StateReport
    {
        public int Collectors;
        public int SessionsAvaliable;
        public int Orders;
        public int MaxPriorityOrders;
        public int MiddlePriorityOrders;
        public int Messages;
        public StateReport(State state, ICommonWriter messagesWriter)
        {
            Collectors = state.Collectors.Count;
            SessionsAvaliable = state.SessionStorages.Count;
            Orders = state.Orders.Count;
            MaxPriorityOrders = state.MaxPriorityOrders.Count;
            MiddlePriorityOrders = state.MiddlePriorityOrders.Count;
            Messages = messagesWriter.GetQueueCount();
        }
    }
}
