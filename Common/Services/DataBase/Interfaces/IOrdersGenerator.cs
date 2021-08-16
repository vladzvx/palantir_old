using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Interfaces
{
    public interface IOrdersGenerator
    {
        public Task CreateUpdatesOrders(CancellationToken token);
        public Task CreateGetNewGroupsOrders(CancellationToken token);
        public Task CreateGetConsistenceSupportingOrders(CancellationToken token);
        public Task CreateGetHistoryOrders(CancellationToken token);
    }
}
