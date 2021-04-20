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
            Thread.Sleep(3000);
            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:5005");
            OrderBoard.OrderBoardClient  Client = new OrderBoard.OrderBoardClient(Channel);
            Order order1 = new Order();
            order1.Id = 1276769488;
            order1.PairLink = "ssle1g";
            order1.Offset = 399;

            //Order order2 = new Order();
            //order2.Id = 1264079104;
            //order2.Link = "ssleg";

            order1.Type = OrderType.History;
            //order2.Type = OrderType.History;


            Client.PostOrder(order1);
            //Client.PostOrder(order2);
            Thread.Sleep(100);
            //while (true)
            {


            }
            
        }
    }
}
