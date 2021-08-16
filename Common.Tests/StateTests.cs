using Common.Services;
using Common.Tests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Common.Services.DataBase.Interfaces;

namespace Common.Tests
{
    [TestClass]
    public class StateTests
    {
        private static OrdersManager manager;
        private static State state;
        private static IOrdersGeneratorMoq generator;
        private static ILimits iLimitsMoq;
        
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            iLimitsMoq = new ILimitsMoq();
            state = new State(new ILoadManagerMoq(), new ILimitsMoq());
            IOrdersGeneratorMoq ordersGenerator = new IOrdersGeneratorMoq(state);
            ordersGenerator.CreateGetNewGroupsOrders(CancellationToken.None).Wait();
        }

        [TestMethod]
        public void TestMethod1()
        {
            int q = 0;
            while (q < 100)
            {
                state.TryGetOrder(new OrderRequest() { Finder = "1", Banned = false }, out Order order) ;
                Assert.IsFalse(order.Type == OrderType.Sleep && q < iLimitsMoq.MaxOrdersNumber);
                Assert.IsTrue((order.Type == OrderType.Pair&&q< iLimitsMoq.MaxOrdersNumber) ||(order.Type == OrderType.Sleep && q >= iLimitsMoq.MaxOrdersNumber));
                q++;
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            int q = 0;
            while (q < 100)
            {
                state.TryGetOrder(new OrderRequest() { Finder = "2", Banned = false }, out Order order);
                Assert.IsFalse(order.Type == OrderType.Sleep && q < iLimitsMoq.MaxOrdersNumber);
                state.TryGetOrder(new OrderRequest() { Finder = "1", Banned = false }, out Order order2);
                Assert.IsTrue(order2.Type== OrderType.Sleep);
                Assert.IsTrue((order.Type == OrderType.Pair && q < iLimitsMoq.MaxOrdersNumber) || (order.Type == OrderType.Sleep && q >= iLimitsMoq.MaxOrdersNumber));
                q++;
            }
        }
    }
}
