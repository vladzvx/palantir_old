using Bot.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class MessagesSender
    {
        private readonly ConcurrentQueue<ISendedItem> sendedItems = new ConcurrentQueue<ISendedItem>();
        private CancellationTokenSource cancellationTokenSource;
        private Task SendingTask;
        public MessagesSender()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }
        public void AddItem(ISendedItem sendedItem)
        {
            if (sendedItem == null) return;
            sendedItems.Enqueue(sendedItem);
            if (SendingTask == null || SendingTask.IsCompleted)
                SendingTask = Send();
        }

        public async Task Send()
        {
            while (sendedItems.TryDequeue(out ISendedItem item))
            {
                await item.Send();
            }
        }
    }
}
