using Bot.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class MessagesSender : IMessagesSender
    {
        private ConcurrentQueue<ISendedItem> sendedItems = new ConcurrentQueue<ISendedItem>();
        private readonly ISenderSettings senderSettings;
        private readonly Task sender;
        public MessagesSender(ISenderSettings senderSettings)
        {
            this.senderSettings = senderSettings;
            sender = Executor(CancellationToken.None);
        }
        public void AddItem(ISendedItem sendedItem)
        {
            if (sendedItem!=null)
                sendedItems.Enqueue(sendedItem);
        }

        private async Task Executor(CancellationToken token)
        {
            Dictionary<long, Task> Buffer = new Dictionary<long, Task>();
            Queue<ISendedItem> Buffer2 = new Queue<ISendedItem>();
            List<ISendedItem> Buffer3 = new List<ISendedItem>();
            List<Task> tasks = new List<Task>();
            while (!token.IsCancellationRequested)
            {
                var keys = Buffer.Keys;
                tasks.RemoveAll(item => item.IsCompleted);
                foreach (var key in keys)
                {
                    if (Buffer[key].IsCompleted)
                    {
                        Buffer.Remove(key);
                    }
                    else
                    {
                        tasks.Add(Buffer[key]);
                    }
                }
                while (Buffer.Count < senderSettings.BufferSize && Buffer2.TryDequeue(out var item))
                {
                    if (!Buffer.ContainsKey(item.ChatId))
                    {
                        Task t = item.Send();
                        Buffer.Add(item.ChatId, t);
                        tasks.Add(t);
                    }
                    else
                    {
                        Buffer3.Add(item);
                    }
                }
                Buffer3.ForEach(item => Buffer2.Enqueue(item));
                Buffer3.Clear();
                while (Buffer.Count < senderSettings.BufferSize && sendedItems.TryDequeue(out var item))
                {
                    if (!Buffer.ContainsKey(item.ChatId))
                    {
                        Task t = item.Send();
                        Buffer.Add(item.ChatId,t);
                        tasks.Add(t);
                    }
                    else
                    {
                        Buffer2.Enqueue(item);
                    }
                }
                if (Buffer2.Count > senderSettings.MaxQueueSize) Buffer2.Clear();
                if (sendedItems.Count > senderSettings.MaxQueueSize) sendedItems.Clear();
                await Task.WhenAll(Task.Delay(senderSettings.MainPeriod), tasks.Count>0?Task.WhenAny(tasks):Task.CompletedTask);
            }
        }
    }
}
