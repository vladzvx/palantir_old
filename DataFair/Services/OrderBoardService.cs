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

        public async override Task DuplexStream(IAsyncStreamReader<OrderResult> requestStream, IServerStreamWriter<OrderReponse> responseStream,ServerCallContext context)
        {
            Task.Factory.StartNew(async ()=>
            {
                int i = 0;
                while (i < 50000)
                {
                    await responseStream.WriteAsync(new OrderReponse() { Text ="From server: "+ i.ToString() });
                    i++;
                }
            });
            
            while (await requestStream.MoveNext())
            {
                OrderResult result = requestStream.Current;
                logger.Debug(result.Text);
                
            }
            return;
        }
        public async override Task<Empty> StreamOrderResults(IAsyncStreamReader<OrderResult> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                OrderResult result = requestStream.Current;
                logger.Info(result.Text);
            }
            return new Empty();
        }
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
            logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id, order.SourceInfo1));
            Orders.Enqueue(order);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> PostOrderResult(OrderResult result, ServerCallContext context)
        {
            logger.Info(result.Text);
            return Task.FromResult(new Empty());
        }

    }
}
