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
    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Order EmptyOrder = new Order() { Type = OrderType.Empty };
        private static ConcurrentQueue<Order> Orders = new ConcurrentQueue<Order>();

        public async override Task ChekingStream(IAsyncStreamReader<Entity> requestStream, IServerStreamWriter<CheckResult> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                await responseStream.WriteAsync(new CheckResult() { Result = true });
            }
            return;
        }
        //public async override Task<Empty> StreamOrderResults(IAsyncStreamReader<OrderResult> requestStream, ServerCallContext context)
        //{
        //    while (await requestStream.MoveNext())
        //    {
        //        OrderResult result = requestStream.Current;
        //        logger.Info(result.Text);
        //    }
        //    return new Empty();
        //}
        public override Task<Order> GetOrder(Empty empty, ServerCallContext context)
        {
            if (Orders.TryDequeue(out Order order))
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
            logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id));
            Orders.Enqueue(order);
            return Task.FromResult(new Empty());
        }

        //public override Task<Empty> PostOrderResult(OrderResult result, ServerCallContext context)
        //{
        //    logger.Info(result.Text);
        //    return Task.FromResult(new Empty());
        //}

    }
}
