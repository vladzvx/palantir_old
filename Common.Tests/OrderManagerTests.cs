using Common.Services;
using Common.Tests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Common.Tests
{
    [TestClass]
    public class OrderManagerTests
    {
        private static OrdersManager manager;
        private static State state;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            state = new State(new ILoadManagerMoq(), new ILimitsMoq());
            IOrdersGeneratorMoq ordersGenerator = new IOrdersGeneratorMoq(state);
            manager = new OrdersManager(state, ordersGenerator, new ISettingsMoq());
            manager.StartAsync(CancellationToken.None).Wait();
        }

        [TestMethod]
        public void TestMethod1()
        {
            manager.heavyOrdersDone = false;
            Thread.Sleep(100);
            Assert.IsTrue(manager.executingState == ExecutingState.UpdatesLoading);
            state.TryGetOrder(new OrderRequest() { Banned = false, Finder = "1" }, out Order order1);
            Assert.IsTrue(order1.Finders.Contains("1"));
            Thread.Sleep(1200);
            Assert.IsTrue(manager.executingState == ExecutingState.HeavyOrdersExecuting);
            state.TryGetOrder(new OrderRequest() { Banned = false, Finder = "1" }, out Order order2);
            Assert.IsTrue(order1.Finders.Contains("1"));
            Thread.Sleep(500);
            Assert.IsTrue(manager.executingState == ExecutingState.HeavyOrdersExecuting);
            state.TryGetOrder(new OrderRequest() { Banned = false, Finder = "1" }, out order2);
            Assert.IsTrue(order2.Finders.Count == 0);
            Thread.Sleep(500);
            Assert.IsTrue(manager.executingState == ExecutingState.HeavyOrdersExecuting);
            Thread.Sleep(1000);
            Assert.IsTrue(manager.heavyOrdersDone);
            Assert.IsTrue(manager.executingState == ExecutingState.UpdatesLoading);
            Thread.Sleep(1200);
            Assert.IsTrue(manager.executingState == ExecutingState.HistoryLoading);
            Thread.Sleep(1200);
            Assert.IsTrue(manager.executingState == ExecutingState.UpdatesLoading);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Thread.Sleep(3000);
            ExecutingState st = manager.executingState;
            int i = 0;
            while (i < 5)
            {
                Thread.Sleep(1050);
                if (st != ExecutingState.OrdersCreation && manager.executingState != ExecutingState.OrdersCreation)
                {
                    Assert.IsTrue((manager.executingState == ExecutingState.HistoryLoading && st == ExecutingState.UpdatesLoading) || (manager.executingState == ExecutingState.UpdatesLoading && st == ExecutingState.HistoryLoading));
                }
                st = manager.executingState;
                i++;
            }
        }
    }
}
