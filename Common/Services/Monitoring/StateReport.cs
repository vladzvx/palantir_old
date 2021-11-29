using Common.Services.DataBase;
using Common.Services.Interfaces;
using System.Collections.Concurrent;
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
        public int Orders;
        public int ConsistanceOrders;
        public int TargetOrders;
        public int Messages;
        public int TotalConnections;
        public int ConnectionsHotReserve;
        public ConcurrentDictionary<string, CounterWrapper> ExecutingOrdersJournal;
        public StateReport(State state, ICommonWriter<Message> commonWriter, ICommonWriter<Entity> commonWriter2, ConnectionsFactory connectionsFactory)
        {
            ConsistanceOrders = state.ConsistanceOrders.Count;
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
            Messages = commonWriter.GetQueueCount();
            TotalConnections = connectionsFactory.TotalConnections;
            ConnectionsHotReserve = connectionsFactory.HotReserve;
            ExecutingOrdersJournal = state.ExecutingOrdersJournal;

            Orders = state.CountOrders();
            TargetOrders = state.CountTargetOrders();
        }
    }
}
