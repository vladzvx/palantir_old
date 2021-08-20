using Common.Services;
using Common.Services.DataBase.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Tests.Services
{
    public class IOrdersGeneratorMoq : IOrdersGenerator
    {
        private readonly State state;
        public IOrdersGeneratorMoq(State state)
        {
            this.state = state;
        }
        public Task CreateGetConsistenceSupportingOrders(CancellationToken token)
        {
            for (int i = 0; i < 1000; i++)
            {
                Order order = new Order();
                order.Type = OrderType.Pair;
                order.Finders.Add("1");
                order.Finders.Add("2");
                state.AddOrder(order);
            }
            return Task.CompletedTask;
        }

        public Task CreateGetHistoryOrders(CancellationToken token)
        {
            for (int i = 0; i < 1000; i++)
            {
                Order order = new Order();
                order.Type = OrderType.History;
                order.Finders.Add("1");
                order.Finders.Add("2");
                order.Finders.Add("3");
                state.AddOrder(order);
            }
            return Task.CompletedTask;
        }

        public Task CreateGetNewGroupsOrders(CancellationToken token)
        {
            for (int i = 0; i < 1000; i++)
            {
                Order order = new Order();
                order.Type = OrderType.Pair;
                state.AddOrder(order);
            }
            return Task.CompletedTask;
        }

        public Task CreateUpdatesOrders(CancellationToken token)
        {
            for (int i = 0; i < 100000; i++)
            {
                Order order = new Order();
                order.Type = OrderType.History;
                order.Finders.Add("1");
                order.Finders.Add("2");
                order.Finders.Add("3");
                state.AddOrder(order);
            }
            return Task.CompletedTask;
        }
    }
}
