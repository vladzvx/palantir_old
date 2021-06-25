using Common;
using Common.Services.DataBase;
using Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Services
{
    public class StateReport
    {
        public int Collectors;
        public int SessionsAvaliable;
        public int Orders;
        public int MaxPriorityOrders;
        public int MiddlePriorityOrders;
        public int Messages;
        public int TotalConnections;
        public int ConnectionsHotReserve;
        public long FreeDisk;
        public StateReport(State state, ICommonWriter<Message> commonWriter, ICommonWriter<Entity> commonWriter2, ConnectionsFactory connectionsFactory)
        {
            foreach (string key in state.Collectors.Keys.ToArray())
            {
                if (state.Collectors.TryGetValue(key,out var val))
                {
                    Collectors += val.Count;
                }
            }
            SessionsAvaliable = state.SessionStorages.Count;
            Orders = state.Orders.Count;
            MaxPriorityOrders = state.MaxPriorityOrders.Count;
            MiddlePriorityOrders = state.MiddlePriorityOrders.Count;
            Messages = commonWriter.GetQueueCount();
            TotalConnections = connectionsFactory.TotalConnections;
            ConnectionsHotReserve = connectionsFactory.HotReserve;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            long disk = 0;
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    disk += d.TotalFreeSpace;
                }
            }
            FreeDisk = disk / 1024 / 1024 / 1024;
        }
    }
}
