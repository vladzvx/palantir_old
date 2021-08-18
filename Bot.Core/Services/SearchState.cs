using Bot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Bot.Core.Services
{
    public class SearchState
    {
        private readonly MessagesSender messagesSender;
        private readonly DBWorker dBWorker;
        private readonly ITelegramBotClient client;

        public SearchState(DBWorker dBWorker, Telegram.Bot.ITelegramBotClient client)
        {
            this.dBWorker = dBWorker;
            this.client = client;
        }
        public async Task SavePage(long user, Page page, CancellationToken token)
        {
            if (page.Number == 0)
            {
                messagesSender.AddItem(page.GetTextMessage(client, user));
            }
            await dBWorker.SavePage(user, page, token);
        }
        
        public async Task TrySendPage(long user, Guid guid, int Number, CancellationToken token)
        {
            Page page = await dBWorker.GetPage(guid, Number, token);
            messagesSender.AddItem(page.GetTextMessage(client, user));
        }

    }
}
