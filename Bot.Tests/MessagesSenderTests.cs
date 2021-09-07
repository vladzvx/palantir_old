using Bot.Core.Interfaces;
using Bot.Core.Services;
using Bot.Tests.Implementations;
using Bot.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Tests
{
    [TestClass]
    public class MessagesSenderTests
    {

        [TestMethod]
        public void PeriodTest1()
        {
            IMessagesSender sender = new MessagesSender(new TestSettings());
            Task t = Task.Factory.StartNew(()=>
            {
                for (int i = 0; i < 10; i++)
                {
                    sender.AddItem(new TestMessage() { ChatId = 0 });
                }

            });
            Thread.Sleep(3000);

            Assert.IsTrue(TestMessage.statistic.Count == 1);
            Assert.IsTrue(TestMessage.statistic[0]==10);
        }

        [TestMethod]
        public void PeriodTest2()
        {
            IMessagesSender sender = new MessagesSender(new TestSettings());
            Task t = Task.Factory.StartNew(() =>
            {
                for (int i1 = 0; i1 < 25; i1++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        sender.AddItem(new TestMessage() { ChatId = i1 });
                    }
                }
            });
            Thread.Sleep(3000);

            Assert.IsTrue(TestMessage.statistic.Count == 25);
            for (int i1 = 0; i1 < 25; i1++)
            {
                Assert.IsTrue(TestMessage.statistic[i1] == 10);
            }
        }

        [TestMethod]
        public void PeriodTest3()
        {
            IMessagesSender sender = new MessagesSender(new TestSettings());
            Task t = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    sender.AddItem(new TestMessage() { ChatId = 0 });
                }
                
            });
            Thread.Sleep(3000);

            Assert.IsTrue(TestMessage.statistic.Count == 1);
            Assert.IsTrue(TestMessage.statistic[0] < 17 && TestMessage.statistic[0] > 12);

        }
    }
}
