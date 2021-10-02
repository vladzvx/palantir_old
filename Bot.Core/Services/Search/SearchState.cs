using Bot.Core.Interfaces;
using Bot.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Bot.Core.Services
{
    public class SearchState
    {
        private readonly IMessagesSender messagesSender;
        private readonly IDataStorage<SearchBot> dBWorker;
        public SearchState(IMessagesSender messagesSender, IDataStorage<SearchBot> dBWorker)
        {
            this.messagesSender = messagesSender;
            this.dBWorker = dBWorker;
        }
        public void SendPage(long user, Page page, CancellationToken token, Channel<int> channel=null)
        {
            if (!string.IsNullOrEmpty(page.Text))
            {
                TextMessage textMessage = page.GetTextMessage(null, user, channel);
                messagesSender.AddItem(textMessage);
            }
        }

        public async Task SavePages(List<Page> pages, CancellationToken token)
        {

            try
            {
                foreach (Page page in pages)
                {
                    page.Text = Encoding.UTF8.GetString(
                        Encoding.Convert(
                            Encoding.Unicode, Encoding.UTF8,
                            Encoding.Unicode.GetBytes(page.Text)
                        ));
                }
                await dBWorker.SavePages(pages, token);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task TryEdit(long chat,int messageId, ObjectId guid, CancellationToken token)
        {
            Page page = await dBWorker.GetPage(guid, token);
            if (page!=null)
                messagesSender.AddItem(page.GetEditTextMessage(null, chat, messageId));
        }

    }
}
