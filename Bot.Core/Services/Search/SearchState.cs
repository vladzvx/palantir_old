using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public class SearchState<TBot> where TBot:IConfig, new()
    {
        private readonly IMessagesSender messagesSender;
        private readonly IDataStorage<TBot> dBWorker;
        public SearchState(IMessagesSender messagesSender, IDataStorage<TBot> dBWorker)
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

        public async Task SendPage(long user, ObjectId pageId, CancellationToken token, Channel<int> channel = null)
        {
            if (TextMessage.defaultClient != null && TextMessage.defaultClient.BotId.HasValue)
            {
                Page page = await dBWorker.GetPage(pageId, token, TextMessage.defaultClient.BotId.Value);
                if (page != null&& !string.IsNullOrEmpty(page.Text))
                    messagesSender.AddItem(page.GetTextMessage(null, user));
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
                if (TextMessage.defaultClient!=null && TextMessage.defaultClient.BotId.HasValue)
                    await dBWorker.SavePages(pages, token, TextMessage.defaultClient.BotId.Value);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task TryEdit(long chat,int messageId, ObjectId guid, CancellationToken token)
        {
            if (TextMessage.defaultClient != null && TextMessage.defaultClient.BotId.HasValue)
            {
                Page page = await dBWorker.GetPage(guid, token,TextMessage.defaultClient.BotId.Value);
                if (page != null)
                    messagesSender.AddItem(page.GetEditTextMessage(null, chat, messageId));
            }

        }
        public async Task TryEdit(Update update, CancellationToken token)
        {
            if (Enum.TryParse<CallbackType>(update.CallbackQuery.Data, out var parsedType))
            {
                if (parsedType== CallbackType.Donate)
                {
                    Message message = update.CallbackQuery.Message;
                    List<IEnumerable<InlineKeyboardButton>> keyb = new List<IEnumerable<InlineKeyboardButton>>();
                    keyb.Add(message.ReplyMarkup.InlineKeyboard.First());
                    foreach (string key in DonateLinks.keyboards.Keys)
                    {
                        if (DonateLinks.keyboards.TryGetValue(key, out LinkInfo linkInfo))
                        {
                            keyb.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton()
                            {CallbackData=linkInfo.Data,Url=linkInfo.Link,Text=linkInfo.Name } });
                        }
                    }
                    EditTextMessage editTextMessage = new EditTextMessage(null, message.Chat.Id, message.Text, message.MessageId,null, 
                        null,0,message.Entities, new InlineKeyboardMarkup(keyb));
                    messagesSender.AddItem(editTextMessage);
                }
            }
        }
    }
}
