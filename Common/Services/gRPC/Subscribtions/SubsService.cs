using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Grpc.Core;


namespace Common.Services.gRPC.Subscribtions
{
    public class SubscribtionService : Subscribtion.SubscribtionBase
    {
        private static readonly ConcurrentDictionary<DateTime,IServerStreamWriter<Message>> MessagesStreams = new ConcurrentDictionary<DateTime, IServerStreamWriter<Message>>();

        public async override Task SubscribeForMessages(SubscribtionRequest order, IServerStreamWriter<Message> serverStream, ServerCallContext context)
        {
            DateTime dateTime = DateTime.UtcNow;
            while (!MessagesStreams.TryAdd(dateTime, serverStream))
            {
                dateTime = DateTime.UtcNow;
            }
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                MessagesStreams.Remove(dateTime,out _);
            }
        }

        public static async Task BroadcastMessage(Message message)
        {
            List<DateTime> ForRemove = new List<DateTime>();
            List<Task> tasks = new List<Task>();
            foreach(DateTime key in MessagesStreams.Keys.ToList())
            {
                try
                {
                    if (MessagesStreams.TryGetValue(key, out var stream))
                    {
                        tasks.Add(stream.WriteAsync(message));
                    }
                }
                catch (Exception e)//Перехватывает исключение, возникающие при попытке отправить оповещение в закрытый поток.
                                    //Может возникнуть при закрытии подключения во время выполнения этого метода (крайне маловероятная ситуация)
                {
                    ForRemove.Add(key);
                }
            }
            if (tasks.Count>0)
                await Task.WhenAll(tasks);
            foreach (DateTime key in ForRemove)
            {
                MessagesStreams.Remove(key, out _);
            }
        }
    }
}
