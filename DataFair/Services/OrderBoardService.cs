using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;

namespace DataFair
{
    class ChachedEntityInfo
    {
        public ChachedEntityInfo(long Id, long AccessHash, long Offset, string Username)
        {
            this.Id = Id;
            this.AccessHash = AccessHash;
            this.Offset = Offset;
            this.Username = Username;
        }

        public string Username;
        public long AccessHash;
        public long Id;
        public long Offset;
        public DateTime LastUpdate= DateTime.UtcNow;
    }


    public static class Storage
    {
        public static ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();
        public static ConcurrentQueue<Entity> Entities = new ConcurrentQueue<Entity>();
        public static ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();
        public static ConcurrentDictionary<long, DateTime> Users = new ConcurrentDictionary<long, DateTime>();
        public static ConcurrentDictionary<long, DateTime> Chats = new ConcurrentDictionary<long, DateTime>();
    }
    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Order EmptyOrder = new Order() { Type = OrderType.Empty };
        public override Task<CheckResult> CheckEntity(Entity entity, ServerCallContext context)
        {
            return Task.FromResult(new CheckResult() {Result = Storage.Users.ContainsKey(entity.Id)|| Storage.Chats.ContainsKey(entity.Id) });
        }

        public override Task<Empty> PostEntity(Entity entity, ServerCallContext context)
        {
            logger.Debug("New entity/ Id: {0}; username: {1}; name: {2}; type: {3};", entity.Id, entity.Link,entity.LastName,entity.Type.ToString());
            switch (entity.Type)
            {
                case EntityType.User:
                    {
                        Storage.Users.TryAdd(entity.Id, DateTime.UtcNow);
                        break;
                    }
                case EntityType.Channel:
                    {
                        Storage.Entities.Enqueue(entity);
                        break;
                    }
                case EntityType.Group:
                    {
                        Storage.Entities.Enqueue(entity);
                        if (entity.Link == string.Empty && entity.PairLink != string.Empty)
                        {
                            Storage.Orders.Enqueue(new Order()
                            {
                                Id = entity.Id,
                                Link = entity.Link,
                                PairId = entity.PairId,
                                PairLink = entity.PairLink,
                                Type = OrderType.History
                            });
                        }

                        break;
                    }
            }
            return Task.FromResult(new Empty());
        }

        public async override Task<Empty> StreamMessages(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Message message = requestStream.Current;
                Storage.Messages.Enqueue(message);
                logger.Debug("Message. DateTime: {0}; FromId: {1}; Text: {2}; Media: {3};", message.Timestamp,message.FromId, message.Text, message.Media);
            }
            return new Empty();
        }
        public override Task<Order> GetOrder(Empty empty, ServerCallContext context)
        {
            if (Storage.Orders.TryDequeue(out Order order))
            {
                
                return Task.FromResult(order);
            }
            else
            {
                logger.Debug("No orders! Sending empty.");
                return Task.FromResult(EmptyOrder);
            }
        }

        public override Task<Empty> PostOrder(Order order, ServerCallContext context)
        {
            logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id,order.Link));
            Storage.Orders.Enqueue(order);
            return Task.FromResult(new Empty());
        }

        //public override Task<Empty> PostOrderResult(OrderResult result, ServerCallContext context)
        //{
        //    logger.Info(result.Text);
        //    return Task.FromResult(new Empty());
        //}

    }
}
