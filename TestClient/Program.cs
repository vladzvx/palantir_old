using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            GrpcChannel Channel = GrpcChannel.ForAddress("http://45.132.17.172:5005");
            OrderBoard.OrderBoardClient  Client = new OrderBoard.OrderBoardClient(Channel);
            while (true)
            {
                var order = Client.GetOrder(new Google.Protobuf.WellKnownTypes.Empty());
                Console.WriteLine(string.Format("{0};{1}",order.Type,order.RedirectCounter));
                //if(order.Type==OrderType.History)
                    Client.PostOrder(order);
                Thread.Sleep(100);
            }
            
        }
    }
}
