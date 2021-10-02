using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Bot.Core.Services.Bot;

namespace Bot.Core.Services
{
    public class SearchReadyProcessor : IReadyProcessor<SearchBot>
    {
        private CancellationTokenSource CancellationTokenSource;
        private readonly IMessagesSender messagesSender;
        private readonly AsyncTaskExecutor asyncTaskExecutor;
        private readonly IServiceProvider serviceProvider;
        public SearchReadyProcessor(IServiceProvider serviceProvider, IMessagesSender messagesSender, AsyncTaskExecutor asyncTaskExecutor)
        {
            this.messagesSender = messagesSender;
            this.serviceProvider = serviceProvider;
            this.asyncTaskExecutor = asyncTaskExecutor;
        }

        public ISendedItem Stop(Update update)
        {
            CancellationTokenSource.Cancel();
            return Bot.Common.CreateOk(update, new ReplyKeyboardRemove(), " Вы можете искать снова.");
        }
        private static string PreparateRequest(string text)
        {
            if (text.Length > 3)
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
            return text;

        }
        private SearchRequest GetRequest(Update update, SearchBot config)
        {
            SearchRequest request = new SearchRequest()
            {
                SearchType = SearchType.SearchNamePeriod,
                IsChannel = config.SearchInChannels,
                IsGroup = config.SearchInGroups,
                Request = PreparateRequest(update.Message.Text),
            };
            DateTime start = DateTime.UtcNow.Date.AddDays(-15 * 365);
            DateTime end = DateTime.UtcNow;
            switch (config.RequestDepth)
            {
                case RequestDepth.Day:
                    {
                        start = DateTime.UtcNow.Date.AddDays(-1);
                        break;
                    }
                case RequestDepth.Week:
                    {
                        start = DateTime.UtcNow.Date.AddDays(-7);
                        break;
                    }
                case RequestDepth.Month:
                    {
                        start = DateTime.UtcNow.Date.AddDays(-30);
                        break;
                    }
                case RequestDepth.Year:
                    {
                        start = DateTime.UtcNow.Date.AddDays(-365);
                        break;
                    }
            }
            request.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(start);
            request.EndTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(end);
            return request;
        }

        public async Task ProcessUpdate(Update update, Bot.FSM<SearchBot> fsm)
        {
            if (!await fsm.TryStartSubFSM(update) && !(asyncTaskExecutor == null))
            {
                fsm.config.BotState = PrivateChatState.Busy;
                CancellationTokenSource = new CancellationTokenSource();
                SearchReciever searchClient = (SearchReciever)serviceProvider.GetService(typeof(SearchReciever));
                Task SearchingTask = searchClient.Search(update.Message.From.Id, GetRequest(update, fsm.config), CancellationTokenSource.Token);
                Task searchFinalsTask = SearchingTask.ContinueWith((_) =>
                {
                    fsm.config.BotState = PrivateChatState.Ready;
                    messagesSender.AddItem(new TextMessage(null, update.Message.Chat.Id, "Завершено!", null, new ReplyKeyboardRemove()));
                    return Task.CompletedTask;
                });

                //Task.WhenAll(SearchingTask, searchFinalsTask).Wait();
                asyncTaskExecutor.Add(Task.WhenAll(SearchingTask, searchFinalsTask));
                messagesSender.AddItem(Bot.Common.CreateOk(update, Bot.Constants.Keyboards.searchingKeyboard));
            }
        }
    }
}
