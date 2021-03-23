using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;

namespace DataFair
{
    struct CachedEntityInfo
    {
        public CachedEntityInfo(long Offset)
        {
            this.Offset = Offset;
            LastTimeMessage = DateTime.UtcNow;
        }
        public long Offset;
        public DateTime LastTimeMessage;
    }

    internal static class Storage
    {
        internal static Timer timer = new Timer(20000);
        internal static DBWorker worker = new DBWorker(Environment.GetEnvironmentVariable("ConnectionString"));


        public static ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();

        public static ConcurrentDictionary<long, CachedEntityInfo> Chats = new ConcurrentDictionary<long, CachedEntityInfo>();

        private static object sync = new object();
        static Storage()
        {
            timer.Elapsed += action;
            timer.AutoReset = true;
            timer.Start();
        }

        private static void action(object sender, ElapsedEventArgs args)
        {
            if (Monitor.TryEnter(sync))
            {
                worker.GetUnupdatedChats(DateTime.UtcNow.AddHours(-24));
                Monitor.Exit(sync);
            }
        }
    }

    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Order EmptyOrder = new Order() { Type = OrderType.Empty };
        public async override Task<CheckResult> CheckEntity(Entity entity, ServerCallContext context)
        {
            return new CheckResult() { Result = await Storage.worker.CheckEntity(entity) };
        }

        public override Task<Empty> PostEntity(Entity entity, ServerCallContext context)
        {
            logger.Debug("New entity/ Id: {0}; username: {1}; name: {2}; type: {3};", entity.Id, entity.Link,entity.LastName,entity.Type.ToString());
            Storage.worker.PutEntity(entity);
            return Task.FromResult(new Empty());
        }

        public async override Task<Empty> StreamMessages(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Storage.worker.PutMessage(requestStream.Current);
                Message message = requestStream.Current;
                var cachedEntityInfo = new CachedEntityInfo(message.Id);
                Storage.Chats.AddOrUpdate(message.ChatId, cachedEntityInfo, (key, old) => cachedEntityInfo);
                logger.Debug("Message. DateTime: {0}; FromId: {1}; Text: {2}; Media: {3};", message.Timestamp,message.FromId, message.Text, message.Media);
            }
            return new Empty();
        }
        public override Task<Order> GetOrder(Empty empty, ServerCallContext context)
        {
            return Task.FromResult(EmptyOrder);
            //if (Storage.Orders.TryDequeue(out Order order))
            //{
            //    if (order.Type == OrderType.History)
            //        Storage.worker.SetChatUpdated(order.Id);
            //    return Task.FromResult(order);
            //}
            //else
            //{
            //   // logger.Debug("No orders! Sending empty.");
            //    return Task.FromResult(EmptyOrder);
            //}
        }

        public override Task<Empty> PostOrder(Order order, ServerCallContext context)
        {
            logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id,order.Link));
            Storage.Orders.Enqueue(order);
            return Task.FromResult(new Empty());
        }
    }
}
