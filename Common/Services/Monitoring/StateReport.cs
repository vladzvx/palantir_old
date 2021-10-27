using Common.Services.DataBase;
using Common.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Services
{
    public class CollectorReport
    {
        public string Phone { get; set; }
        public string Group { get; set; }
    }
    public class StateReport
    {
        public int CollectorsCount;
        public List<CollectorReport> Collectors = new List<CollectorReport>();
        public int SessionsAvaliable;
        public int Orders;
        public int MaxPriorityOrders;
        public int MiddlePriorityOrders;
        public int TargetOrders;
        public int Messages;
        public int TotalConnections;
        public int ConnectionsHotReserve;
        public long FreeDisk;
        public string OrderManagerState;
        public StateReport(State state, ICommonWriter<Message> commonWriter, ICommonWriter<Entity> commonWriter2, ConnectionsFactory connectionsFactory)
        {
            foreach (string key in state.Collectors.Keys.ToArray())
            {
                if (state.Collectors.TryGetValue(key, out var val))
                {
                    CollectorsCount += val.Count;
                    foreach (var coll in val)
                    {
                        Collectors.Add(new CollectorReport() {Phone= coll.Phone,Group=key } );
                    }
                }

            }
            SessionsAvaliable = state.SessionStorages.Count;
            Messages = commonWriter.GetQueueCount();
            TotalConnections = connectionsFactory.TotalConnections;
            ConnectionsHotReserve = connectionsFactory.HotReserve;


            OrderManagerState = state.ordersManager.executingState.ToString();
            Orders = state.CountOrders();
            TargetOrders = state.CountTargetOrders();

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
