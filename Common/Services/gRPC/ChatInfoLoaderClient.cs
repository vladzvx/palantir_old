using Common.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.gRPC
{
    public class ChatInfoLoaderClient
    {
        private readonly IGrpcSettings grpcSettings;
        private readonly ICommonWriter<Entity> commonWriter;
        public ChatInfoLoaderClient(IGrpcSettings grpcSettings, ICommonWriter<Entity> commonWriter)
        {
            this.grpcSettings = grpcSettings;
            this.commonWriter = commonWriter;

        }
        public async Task GetChats()
        {
            using (GrpcChannel grpcChannel = GrpcChannel.ForAddress(grpcSettings.Url))
            {
                OrderBoard.OrderBoardClient boardClient = new OrderBoard.OrderBoardClient(grpcChannel);
                var temp = boardClient.GetChats(new Empty());
                while (await temp.ResponseStream.MoveNext())
                {
                    commonWriter.PutData(temp.ResponseStream.Current);
                }
            }

        }
    }
}
