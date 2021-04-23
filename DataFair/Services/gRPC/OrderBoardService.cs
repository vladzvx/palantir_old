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
using System.Diagnostics;
using System.IO;

namespace DataFair.Services
{
    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Order EmptyOrder = new Order() { Type = OrderType.Empty };
        private readonly State state;
        public OrderBoardService(State state)
        {
            this.state = state;
        }
        public override Task<Empty> PostEntity(Entity entity, ServerCallContext context)
        {
            logger.Trace("New entity Id: {0}; username: {1}; name: {2}; type: {3};", entity.Id, entity.Link,entity.LastName,entity.Type.ToString());
            state.worker.PutEntity(entity);
            return Task.FromResult(new Empty());
        }

        public async override Task<Empty> StreamMessages(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext())
                {
                    state.worker.PutMessage(requestStream.Current);
                    Message message = requestStream.Current;
                    logger.Trace("Message. DateTime: {0}; FromId: {1}; Text: {2}; Media: {3};", message.Timestamp, message.FromId, message.Text, message.Media);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while recieving messages stream!");
            }
            return new Empty();
        }
        public override Task<Order> GetOrder(Empty empty, ServerCallContext context)
        {
            try
            {
                if (state.Orders.TryDequeue(out Order order))
                {
                    return Task.FromResult(order);
                }
                else
                {
                    return Task.FromResult(EmptyOrder);
                }
            }
            catch (Exception ex )
            {
                logger.Error(ex,"Error while executing GetOrder request");
                return Task.FromResult(EmptyOrder);
            }
        }
        public override Task<Empty> PostOrder(Order order, ServerCallContext context)
        {
            try
            {
                logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id, order.Link));
                state.Orders.Enqueue(order);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while PostOrder execution!");
            }
            return Task.FromResult(new Empty());
        }
        public override Task<StateReport> GetState(Empty request, ServerCallContext context)
        {
            return Task.FromResult(state.GetStateReport());
        }

    }
}
