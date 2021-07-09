using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public class MessagesSender
    {
        private readonly ConcurrentQueue<SearchResult> searchResults = new ConcurrentQueue<SearchResult>();
       // private readonly ConcurrentQueue<MessageEntity> messagesEnt = new ConcurrentQueue<MessageEntity>();
        private readonly object locker = new object();
        private string MessageText = string.Empty;
        private string MessageTextOld = string.Empty;
        private List<MessageEntity> entities = new List<MessageEntity>();
        private Message mess = null;
        private readonly ITelegramBotClient botClient;
        private DateTime lastSendingTime;
        private ChatId chatId;
        public Task sendingTask;
        private bool complited = false;
        private int avaryLimit;
        public MessagesSender(ITelegramBotClient botClient, ChatId chatId, CancellationToken token, int limit = 150)
        {
            avaryLimit = limit;
            this.botClient = botClient;
            this.chatId = chatId;
            lastSendingTime = DateTime.UtcNow;
            sendingTask = Task.Factory.StartNew((tok) => 
            {
                if (tok is not CancellationToken tok2) return;
                int count = 1;
                int offset = 0;
                while ((!IsComplited || !searchResults.IsEmpty)&& !tok2.IsCancellationRequested)
                {
                    DateTime sendingTime = DateTime.UtcNow;
                    lock (locker)
                    {
                        while (MessageText.Length < 3700 && searchResults.TryDequeue(out SearchResult res))
                        {
                            string line = count.ToString() + ". " + (res.Text.Length > 100 ? res.Text.Substring(0, 99) + "..." : res.Text) + "\n\n";
                            MessageEntity formatting = new MessageEntity() { Length = count.ToString().Length, Url = res.Link, Offset = offset, Type = MessageEntityType.TextLink };
                            offset += line.Length;

                            count++;
                            MessageText += line;
                            entities.Add(formatting);
                            if (count > avaryLimit)
                            {
                                break;
                            }
                        }
                        if (!MessageTextOld.Equals(MessageText))
                        {
                            if (mess == null)
                            {
                                mess = botClient.SendTextMessageAsync(chatId, MessageText, entities: entities).Result;
                                MessageTextOld = MessageText;
                            }
                            else
                            {
                                botClient.EditMessageTextAsync(chatId, mess.MessageId, MessageText, entities: entities).Wait();
                                MessageTextOld = MessageText;
                            }
                        }
                        if (count > avaryLimit)
                        {
                            IsComplited = true;
                            return;
                        }
                        if (MessageText.Length > 3700)
                        {
                            mess = null;
                            MessageText = string.Empty;
                            entities.Clear();
                            offset = 0;
                        }
                    }
                    Task.Delay(2000).Wait();
                }
            }, token, TaskCreationOptions.LongRunning);
        }

        public void Send(SearchResult searchResult)
        {
            searchResults.Enqueue(searchResult);
            //lines.Enqueue(line);
            //messagesEnt.Enqueue(messageEntity);
        }
        public bool IsComplited
        {
            get
            {
                lock (locker)
                {
                    return complited;
                }
            }
            set
            {
                lock (locker)
                {
                    complited = value;
                }
            }
        }
    }
}
