using Common;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.gRPC;
using Grpc.Core;
using Grpc.Net.Client;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public class BotMessageHandler : IUpdateHandler
    {
        internal ConcurrentDictionary<long, CancellationTokenSource> sources = new ConcurrentDictionary<long, CancellationTokenSource>();
        internal ConcurrentDictionary<long, string> setters = new ConcurrentDictionary<long, string>();

        internal static IServiceProvider serviceProvider;

        public UpdateType[] AllowedUpdates => new UpdateType[] { UpdateType.Message};

        public BotMessageHandler()
        {
            
            
        }
        public async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

        }

        private CancellationTokenSource GetCTS(Update update)
        {
            CancellationTokenSource cancellationTokenSource = null;
            if (sources.ContainsKey(update.Message.From.Id))
            {
                if (sources.TryRemove(update.Message.From.Id, out cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                }
            }
            cancellationTokenSource = new CancellationTokenSource();
            sources.TryAdd(update.Message.From.Id, cancellationTokenSource);
            return cancellationTokenSource;
        }

        private SearchRequest GetRequest(Update update)
        {
            SearchRequest request = new SearchRequest()
            {
                SearchType = SearchType.SearchNamePeriod,
                IsChannel = true,
                IsGroup = true,
                Limit = 50,
                Request = PreparateRequest(update.Message.Text),
            };
            DateTime start = DateTime.UtcNow.Date.AddDays(-365);
            DateTime end = DateTime.UtcNow;
            if (setters.TryGetValue(update.Message.From.Id, out string val))
            {
                if (val == "День")
                {
                    start =DateTime.UtcNow.Date.AddDays(-1);
                }
                else if (val == "Неделя")
                {
                    start = DateTime.UtcNow.Date.AddDays(-7);
                }
                else if (val == "Месяц")
                {
                    start = DateTime.UtcNow.Date.AddDays(-30);
                }
            }
            request.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(start);
            request.EndTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(end);
            return request;
        }
        private async Task<bool> ParseCommand(ITelegramBotClient botClient, Update update)
        {

            if (update.Message.Text == "/settings"|| update.Message.Text == "/start")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> { 
                    new List<KeyboardButton>() { "День" },
                    new List<KeyboardButton>(){ "Неделя" },
                    new List<KeyboardButton>(){ "Месяц" },});
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Выберете интервал поиска",replyMarkup: replyKeyboardMarkup);
                setters.TryRemove(update.Message.From.Id, out _);
                setters.TryAdd(update.Message.From.Id, null);
                return false;
            }
            else if (update.Message.Text[0] == '/')
            {
                return false;
            }

            if (setters.TryGetValue(update.Message.From.Id, out var t))
            {
                if (t == null)
                {
                    if (update.Message.Text == "День" || update.Message.Text == "Неделя" || update.Message.Text == "Месяц")
                    {
                        setters.TryRemove(update.Message.From.Id, out _);
                        setters.TryAdd(update.Message.From.Id, update.Message.Text);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Принято!", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Настройка глубины поиска не распознана, установлен интервал по умолчанию: последний год");
                        setters.TryRemove(update.Message.From.Id, out _);
                        setters.TryAdd(update.Message.From.Id, string.Empty);
                    }
                    return false;
                }
                else return true;
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Наберите команду /settings для выбора настроек глубины поиска!", replyMarkup: new ReplyKeyboardRemove());
                return false;
            }
            return true;
        }
        private string PreparateRequest(string text)
        {
            string[] words = text.Split(' ');
            string requesr = string.Empty;
            foreach (string word in words)
            {
                if (word.Length > 3)
                {
                    requesr += word + '&';
                }
            }
            return requesr.Remove(requesr.Length - 1);
            
        }
        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                
                if (await ParseCommand(botClient,update))
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Запрос принят!",replyToMessageId: update.Message.MessageId);
                        CancellationTokenSource cancellationTokenSource = GetCTS(update);

                        SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                        MessagesSender messagesSender = new MessagesSender(botClient, update.Message.Chat.Id, cancellationTokenSource.Token);

                        SearchRequest request = GetRequest(update);

                        Task SearchingTask = Task.Factory.StartNew(() => searchClient.Search(request, cancellationTokenSource.Token).Wait());

                        DateTime lastResul = DateTime.UtcNow;
                        int count = 0;
                        while (!messagesSender.IsComplited)
                        {
                            while (searchClient.searchResultReciever.TryDequeueResult(out SearchResult res))
                            {
                                count++;
                                messagesSender.Send(res);
                                lastResul = DateTime.UtcNow;
                                if (count > request.Limit)
                                {
                                   // sources.TryRemove(update.Message.From.Id, out cancellationTokenSource);
                                    //cancellationTokenSource.Cancel();
                                   // messagesSender.IsComplited = true;
                                    //return;
                                }
                            }
                            if (SearchingTask.IsCompleted&& DateTime.UtcNow.Subtract(lastResul).TotalSeconds>10)
                            {
                                messagesSender.IsComplited = true;
                            }
                            if (DateTime.UtcNow.Subtract(lastResul).TotalSeconds > 120)
                            {
                                botClient.SendTextMessageAsync(update.Message.Chat.Id, "Превышено максимальное время ожидания результатов, попробуйте другой запрос!").Wait();
                                break;
                            }
                            //messagesSender.IsComplited = searchClient.searchResultReciever.IsComplited;
                            await Task.Delay(200);
                        }

                        if(sources.TryRemove(update.Message.From.Id, out cancellationTokenSource))
                        {
                            cancellationTokenSource.Cancel();
                        }
                        botClient.SendTextMessageAsync(update.Message.Chat.Id, "Поиск завершен!").Wait();

                    }
                    catch (Exception ex)
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, ex.Message);
                        await Task.Delay(1000);
                    }
                }



            }


        }
    }
}
