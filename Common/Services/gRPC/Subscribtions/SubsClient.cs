using Common.Services.DataBase.Reading;
using Common.Services.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.gRPC.Subscribtions
{
    public class GrpcDataReciever : IHostedService
    {
        private readonly IGrpcSettings grpcSettings;
        private GrpcChannel Channel;
        private Subscribtion.SubscribtionClient Client;
        private AsyncServerStreamingCall<Message> call;
        private Task subscribtionTask;
        private Task reconnectionTask;
        private readonly ICommonWriter<Message> commonWriter;
        private readonly ChatInfoLoaderClient chatInfoLoader;
        public GrpcDataReciever(IGrpcSettings grpcSettings, ICommonWriter<Message> commonWriter, ChatInfoLoaderClient chatInfoLoader)
        {
            this.grpcSettings = grpcSettings;
            this.commonWriter = commonWriter;
            this.chatInfoLoader = chatInfoLoader;
        }

        public async Task Subscribe()
        {
            try
            {
                call = Client.SubscribeForMessages(new SubscribtionRequest() { });
                while (await call.ResponseStream.MoveNext())
                {
                    Message mess = call.ResponseStream.Current;
                    commonWriter.PutData(mess);
                    //Console.WriteLine(string.Format("New Message! " +
                    //    "ChatId: {0}; MessageId: {1}; Text: {2};", mess.ChatId, mess.Id, mess.Text));
                }
            }
            catch (Grpc.Core.RpcException ex)
            {
                call.Dispose();
            }

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //await chatInfoLoader.GetChats();
            Channel = GrpcChannel.ForAddress(grpcSettings.Url);
            Client = new Subscribtion.SubscribtionClient(Channel);
            //await Subscribe();
            subscribtionTask = Subscribe();
            reconnectionTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await subscribtionTask;
                    await Task.Delay(5000);
                    subscribtionTask = Subscribe();
                }
            });
            //return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Channel.Dispose();
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
