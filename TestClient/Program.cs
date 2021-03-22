﻿using Common;
using Grpc.Net.Client;
using System;
using System.Net.Http;

namespace TestClient
{
    class Program
    {
        private static GrpcChannel Channel;
        private static OrderBoard.OrderBoardClient Client;
        static void Main(string[] args)
        {
            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            Channel = GrpcChannel.ForAddress("http://176.119.156.220:5005", new GrpcChannelOptions { HttpHandler = httpHandler });
            Client = new OrderBoard.OrderBoardClient(Channel);
            var result = Client.GetOrder(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}