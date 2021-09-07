using Bot.Core.Interfaces;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class MessagesSender : IMessagesSender
    {
        private readonly ConcurrentQueue<ISendedItem> sendedItems = new ConcurrentQueue<ISendedItem>();
        private readonly CancellationTokenSource cancellationTokenSource;
        private Task SendingTask;
        public MessagesSender()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }
        public void AddItem(ISendedItem sendedItem)
        {
            if (sendedItem == null)
            {
                return;
            }

            sendedItems.Enqueue(sendedItem);
            if (SendingTask == null || SendingTask.IsCompleted)
            {
                SendingTask = Send();
            }
        }

        private async Task Send()
        {
            while (sendedItems.TryDequeue(out ISendedItem item))
            {
                await item.Send();
            }
        }
    }
}
