using Common.Models;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.gRPC
{
    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Order EmptyOrder = new Order() { Type = OrderType.Empty };
        private static readonly Order Sleep = new Order() { Type = OrderType.Sleep, Time = 10 };
        private readonly State ordersStorage;
        private readonly ICommonWriter commonWriter;
        private readonly ICommonWriter<Message> messagesWriter;
        private readonly ICommonWriter<Entity> entitiesWriter;
        private readonly ICommonReader<ChatInfo> commonReader;
        public OrderBoardService(State ordersStorage, ICommonWriter commonWriter, ICommonWriter<Message> messagesWriter,
            ICommonWriter<Entity> entitiesWriter, ICommonReader<ChatInfo> commonReader)
        {
            this.ordersStorage = ordersStorage;
            this.commonWriter = commonWriter;
            this.entitiesWriter = entitiesWriter;
            this.messagesWriter = messagesWriter;
            this.commonReader = commonReader;
        }

        public async override Task<ChatInfo> GetChatInfo(ChatInfoRequest request, ServerCallContext context)
        {
            return await commonReader.ReadAsync(request, CancellationToken.None);
        }
        public override Task<Empty> PostEntity(Entity entity, ServerCallContext context)
        {
            logger.Trace("New entity Id: {0}; username: {1}; name: {2}; type: {3};", entity.Id, entity.Link, entity.LastName, entity.Type.ToString());
            if (entity.Type == EntityType.Channel || entity.Type == EntityType.Group)
            {
                entitiesWriter.PutData(entity);
            }
            else if (User.TryCast(entity, out User user))
            {
                commonWriter.PutData(user);
            }
            else if (Ban.TryCast(entity, out Ban ban))
            {
                commonWriter.PutData(ban);
            }
            return Task.FromResult(new Empty());
        }
        public async override Task<Empty> StreamMessages(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
        {
            try
            {
                long maxId = 0;
                long chatId = 0;
                while (await requestStream.MoveNext())
                {
                    messagesWriter.PutData(requestStream.Current);
                    //await loadManager.WaitIfNeed();
                    if (requestStream.Current.Id > maxId)
                    {
                        maxId = requestStream.Current.Id;
                    }

                    chatId = requestStream.Current.ChatId;
                    //Message message = requestStream.Current;
                    //logger.Trace("Message. DateTime: {0}; FromId: {1}; Text: {2}; Media: {3};", message.Timestamp, message.FromId, message.Text, message.Media);
                }
                if (maxId != 0)
                {
                    await entitiesWriter.ExecuteAdditionalAction(new Order() { Id = chatId, Offset = maxId, Type = OrderType.Container });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while recieving messages stream!");
            }
            return new Empty();
        }
        public override Task<Order> GetOrder(OrderRequest req, ServerCallContext context)
        {
            try
            {
                if (ordersStorage.TryGetOrder(req, out Order resOrder))
                {
                    return Task.FromResult(resOrder);
                }
                else
                {
                    return Task.FromResult(EmptyOrder);
                }
                //if (string.IsNullOrEmpty(req.Finder))
                //{
                //    Order order = EmptyOrder;
                //    if (ordersStorage.MaxPriorityOrders.TryDequeue(out Order order1))
                //    {
                //        order = order1;
                //        return Task.FromResult(order);
                //    }
                //    else if (ordersStorage.MiddlePriorityOrders.TryDequeue(out Order order2))
                //    {
                //        order = order2;
                //        return Task.FromResult(order);
                //    }
                //    else if (ordersStorage.Orders.TryDequeue(out Order order3))
                //    {
                //        order = order3;
                //        return Task.FromResult(order);
                //    }
                //}
                //if (!string.IsNullOrEmpty(req.Finder))
                //{
                //    for (int i = 0; i < 100; i++)
                //    {
                //        if (ordersStorage.Orders.TryDequeue(out Order order))
                //        {
                //            if (order.Finders.Contains(req.Finder))
                //            {
                //                return Task.FromResult(order);
                //            }
                //            else
                //            {
                //                ordersStorage.Orders.Enqueue(order);
                //            }
                //        }
                //    }
                //}

                //return Task.FromResult(Sleep);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while executing GetOrder request");
                return Task.FromResult(EmptyOrder);
            }
        }
        public async override Task<Empty> PostOrder(Order order, ServerCallContext context)
        {
            try
            {
                logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id, order.Link));
                if (order.Type == OrderType.GetFullChannel)
                {
                    ordersStorage.MaxPriorityOrders.Enqueue(order);
                }
                else if (order.Type == OrderType.Executed)
                {
                    await entitiesWriter.ExecuteAdditionalAction(order);
                }
                else
                {
                    ordersStorage.MiddlePriorityOrders.Enqueue(order);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while PostOrder execution!");
            }
            return new Empty();
        }
        public override Task<Empty> PostReport(Report report, ServerCallContext context)
        {
            try
            {
                logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", report.SourceId, report.Type));
                ordersStorage.Reports.Enqueue(report);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while PostOrder execution!");
            }
            return Task.FromResult(new Empty());
        }
    }
}
