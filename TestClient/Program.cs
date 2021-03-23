using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Threading;

namespace TestClient
{
    class Program
    {
        private static GrpcChannel Channel;
        private static OrderBoard.OrderBoardClient Client;
        static void Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Thread.Sleep(2000);
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            Channel = GrpcChannel.ForAddress("http://176.119.156.220:5015",new GrpcChannelOptions() {HttpHandler= httpHandler,Credentials=ChannelCredentials.Insecure });
            //Channel = GrpcChannel.ForAddress("https://localhost:5005");
            Client = new OrderBoard.OrderBoardClient(Channel);
            var result = Client.GetOrder(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
