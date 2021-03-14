using Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StreamingBidir.Client
{
    class Program
    {
        //private static long Id;
        //private static long SourceInfo2 = 120000;

        //private static Random rnd = new Random();
        //private static Timer ExecutorTimer = new Timer(1000);
        //private static GrpcChannel Channel;
        //private static OrderBoard.OrderBoardClient Client;
        //private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //static async void ExecuteOrder(object sender, ElapsedEventArgs args)
        //{
        //    Order order = await Client.GetOrderAsync(new Empty());
        //    if (order.Type == OrderType.ClientStreaming)
        //    {
        //        logger.Debug(string.Format("Streaming by order Started! Id: {0}; Field: {1}; Iterations: {2}", order.Id, order.SourceInfo1, order.SourceInfo2));
        //        var call = Client.DuplexStream();
        //        Task.Factory.StartNew(async ()=> 
        //        {
        //            while (await call.ResponseStream.MoveNext())
        //            {
        //                logger.Debug(call.ResponseStream.Current.Text);
        //            }
        //        });
        //        int i = 0;
        //        while (i < order.SourceInfo2)
        //        {
        //            string text = string.Format("From Client. Iteration №{0}", i);
        //            await call.RequestStream.WriteAsync(new OrderResult() { OrderId = order.Id, Text = text });
        //            //logger.Trace(text);
        //            i++;
        //        }
        //    }
        //    else
        //    {
        //        logger.Debug(string.Format("OrderType is wrong! Creating test order for {0} iterations.", SourceInfo2));
        //        Client.PostOrder(new Order() { Id = Id, SourceInfo2 = SourceInfo2, Type = OrderType.ClientStreaming });
        //    }
        //}


        static void Main(string[] args)
        {
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            //System.Threading.Thread.Sleep(1000);
            //Id = rnd.Next();
            //var httpHandler = new HttpClientHandler();
            //httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            //Channel = GrpcChannel.ForAddress("http://176.119.156.220:5005", new GrpcChannelOptions { HttpHandler = httpHandler });
            //Client = new OrderBoard.OrderBoardClient(Channel);

            //ExecutorTimer.Elapsed += ExecuteOrder;
            //ExecutorTimer.AutoReset = true;
            //ExecutorTimer.Start();

            //Console.ReadKey();
        }
    }
}
