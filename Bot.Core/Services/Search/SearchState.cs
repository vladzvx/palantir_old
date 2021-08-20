using Bot.Core.Models;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Bot.Core.Services
{
    public class SearchState
    {
        private readonly MessagesSender messagesSender;
        private readonly DBWorker dBWorker;
        private readonly ITelegramBotClient client;

        public SearchState(MessagesSender messagesSender, DBWorker dBWorker)
        {
            this.messagesSender = messagesSender;
            this.dBWorker = dBWorker;
        }
        public async Task SavePage(long user, Page page, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(page.Text))
            {
                if (page.Number == 0)
                {
                    Channel<int> channel = Channel.CreateBounded<int>(1);
                    TextMessage textMessage = page.GetTextMessage(client, user, channel);
                    messagesSender.AddItem(textMessage);
                    page.MessageNumber = await channel.Reader.ReadAsync();
                }
                await dBWorker.SavePage(page, token);
            }
        }

        public async Task TrySendPage(long user, Guid guid, int Number, CancellationToken token)
        {
            Page page = await dBWorker.GetPage(guid, Number, token);
            messagesSender.AddItem(page.GetEditTextMessage(client, user));
        }

    }
}
