using Bot.Core.Interfaces;
using Bot.Core.Services;
using Common;
using Common.Services.gRPC;
using Common.Services.Interfaces;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public static class Constants
    {
        public static ImmutableList<string> Cancells = ImmutableList.CreateRange(new string[1] { "отмена" });
        public static ImmutableList<string> CallSettings = ImmutableList.CreateRange(new string[1] { "/settings" });
        public static ImmutableList<string> Day = ImmutableList.CreateRange(new string[1] { "день" });
        public static ImmutableList<string> Week = ImmutableList.CreateRange(new string[1] { "неделя" });
        public static ImmutableList<string> Month = ImmutableList.CreateRange(new string[1] { "месяц" });
        public static ImmutableList<string> Year = ImmutableList.CreateRange(new string[1] { "год" });
    }

    public class Bot :IDisposable
    {
        private readonly protected TelegramBotClient botClient;
        private readonly CancellationToken cancellationToken;
        public Bot(IBotSettings settings, IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
        {
            botClient = new TelegramBotClient(settings.Token);
            cancellationToken = cancellationTokenSource.Token;
            FinitStateMachine.serviceProvider = serviceProvider;
        }
        public void Start()
        {
            botClient.StartReceiving<BotMessageHandler2>(cancellationToken);
        }
        public void Dispose()
        {
            botClient.StartReceiving<BotMessageHandler>();
        }

        #region subclasses

        public enum State
        {
            Started = 0,
            ConfiguringDepth = 1,
            ConfiguringGroups = 4,
            ConfiguringChannel = 5,
            ConfiguringLimit = 6,
            Ready = 2,
            Searching = 3
        }

        public enum RequestDepth
        {
            Inf,
            Day,
            Week,
            Month,
            Year
        }


        public class FinitStateMachine
        {
            public static IServiceProvider serviceProvider;
            public readonly string Token;
            public readonly ITelegramBotClient botClient;
            public readonly ChatId chatId;
            private State _state;
            private readonly object stateLocker = new object();
            private readonly object lastMessageLocker = new object();
            private readonly object depthLocker = new object();
            private readonly object settingsLocker = new object();
            private ReplyKeyboardMarkup settingKeyboard;
            private ReplyKeyboardMarkup yesNoKeyboard;
            private ReplyKeyboardMarkup searchingKeyboard;
            private ReplyKeyboardMarkup limtKeyboard;
            //private IReplyMarkup mainKeyboard;
            private CancellationTokenSource CancellationTokenSource;
            private Task SearchingTask;
            private Message _lastMessage;
            private bool _searchInGroups = true;
            private bool _searchInChannels = true;
            private int _limit=150;
            private RequestDepth _requestDepth;

            public FinitStateMachine(ITelegramBotClient botClient, ChatId chatId)
            {
                this.botClient = botClient;
                this.chatId = chatId;
                InitKeyboards();
            }

            private void InitKeyboards()
            {
                settingKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "День" },
                    new List<KeyboardButton>(){ "Неделя" },
                    new List<KeyboardButton>(){ "Месяц" },
                    new List<KeyboardButton>(){ "Год" },
                }, true, true);

                yesNoKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "Да" },
                    new List<KeyboardButton>(){ "Нет" },
                }, true, true);

                limtKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "10" },
                    new List<KeyboardButton>(){ "20" },
                    new List<KeyboardButton>(){ "50" },
                    new List<KeyboardButton>(){ "150" },
                    new List<KeyboardButton>(){ "1000" },
                }, true, true);

                searchingKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "Отмена" } },true,true);

                //mainKeyboard = new ReplyKeyboardRemove();
            }
            private static string PreparateRequest(string text)
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
            private SearchRequest GetRequest(Update update)
            {
                SearchRequest request = new SearchRequest()
                {
                    SearchType = SearchType.SearchNamePeriod,
                    IsChannel = SearchInGroups,
                    IsGroup = SearchInChannels,
                    Limit = Limit,
                    Request = PreparateRequest(update.Message.Text),
                };
                DateTime start = DateTime.UtcNow.Date.AddDays(-15 * 365);
                DateTime end = DateTime.UtcNow;
                switch (RequestDepth)
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
                            start = DateTime.UtcNow.Date.AddDays(-30);
                            break;
                        }
                }
                request.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(start);
                request.EndTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(end);
                return request;
            }

            private async Task ParseDepth(Update update)
            {
                string tmp = update.Message.Text.ToLower();
                if (Constants.Day.Contains(tmp))
                {
                    RequestDepth = RequestDepth.Day;
                }
                else if (Constants.Week.Contains(tmp))
                {
                    RequestDepth = RequestDepth.Week;
                }
                else if (Constants.Month.Contains(tmp))
                {
                    RequestDepth = RequestDepth.Month;
                }
                else if (Constants.Year.Contains(tmp))
                {
                    RequestDepth = RequestDepth.Year;
                }
                else
                {
                    RequestDepth = RequestDepth.Inf;
                }
                
            }

            private async Task<bool> ParseConfiguring(Update update)
            {
                if (Constants.CallSettings.Contains(update.Message.Text.ToLower()))
                {
                    state = State.ConfiguringDepth;
                    LastBotMessage = await botClient.SendTextMessageAsync(chatId,"Выберите глубину поиска",replyMarkup:settingKeyboard);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private async Task Ok(Update update, IReplyMarkup keyboard=null,string additionalMessage = "")
            {
                LastBotMessage = await botClient.SendTextMessageAsync(chatId, "Принято!"+additionalMessage, replyMarkup: keyboard, replyToMessageId: update.Message.MessageId);
            }


            #region searching processing
            private async Task ImBusy()
            {
                LastBotMessage = await botClient.SendTextMessageAsync(chatId, "Идет поиск, но вы можете отменить его.", replyMarkup: searchingKeyboard);
            }
            private async Task SearchingProcessing(Update update)
            {
                if (Constants.Cancells.Contains(update.Message.Text.ToLower()))
                {
                    CancellationTokenSource.Cancel();
                    await Ok(update, new ReplyKeyboardRemove()," Вы можете искать снова.");
                    state = State.Ready;
                }
                else
                {
                    await ImBusy();
                }
            }
            public async Task ReadyProcessing(Update update)
            {
                if (await ParseConfiguring(update))
                {
                    await Ok(update, searchingKeyboard);
                    state = State.Searching;
                    CancellationTokenSource = new CancellationTokenSource();
                    SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                    SearchingTask = searchClient.Search(GetRequest(update), CancellationTokenSource.Token);
                    MessagesSender messagesSender = new MessagesSender(botClient, update.Message.Chat.Id, CancellationTokenSource.Token, Limit);               
                    Task SendingTask = Task.Factory.StartNew(async () =>
                    {
                        int count = 0;
                        CancellationToken token = CancellationTokenSource.Token;
                        while (!searchClient.searchResultReciever.IsComplited && !token.IsCancellationRequested)
                        {
                            await Task.Delay(200);
                            while (searchClient.searchResultReciever.TryDequeueResult(out SearchResult res))
                            {
                                messagesSender.Send(res);
                                count++;
                            }
                        }
                        if (count == 0)
                        {
                            LastBotMessage =await botClient.SendTextMessageAsync(chatId, "Ничего не найдено, попробуйте другой запрос.", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: CancellationTokenSource.Token);
                            CancellationTokenSource.Cancel();
                            state = State.Ready;
                        }
                    });
                    Task.Factory.StartNew(() =>
                    {
                        Task.WaitAll(SendingTask, SearchingTask);
                        if (messagesSender.sendingTask != null)
                        {
                            messagesSender.sendingTask.ContinueWith((prevState) => 
                            {
                                state = State.Ready;
                                LastBotMessage = botClient.SendTextMessageAsync(chatId, "Поиск завершен!", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: CancellationTokenSource.Token).Result;
                            });
                        }

                    });
                }
            }
            #endregion
            public async Task ProcessUpdate(Update update)
            {
                try
                {
                    switch (state)
                    {
                        case State.Started:
                            {
                                if (await ParseConfiguring(update))
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Настройте глубину поиска, набрав команду /settings");
                                }
                                break;
                            }
                        case State.ConfiguringDepth:
                            {
                                await ParseDepth(update);
                                await Ok(update, yesNoKeyboard," Искать в группах?");
                                state = State.ConfiguringGroups;
                                break;
                            }
                        case State.ConfiguringGroups:
                            {
                                SearchInGroups = update.Message.Text.ToLower() == "да";
                                await Ok(update, yesNoKeyboard, " Искать в каналах?");
                                state = State.ConfiguringChannel;
                                break;
                            }
                        case State.ConfiguringChannel:
                            {
                                SearchInChannels = update.Message.Text.ToLower() == "да";
                                await Ok(update, limtKeyboard, " Выберете предельное число результатов (по умолчанию - 150)");
                                state = State.ConfiguringLimit;
                                break;
                            }
                        case State.ConfiguringLimit:
                            {
                                if (int.TryParse(update.Message.Text.ToLower(), out int res))
                                {
                                    await Ok(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                                    state = State.Ready;
                                    Limit = res;
                                }
                                break;
                            }
                        case State.Ready:
                            {
                                await ReadyProcessing(update);
                                break;
                            }
                        case State.Searching:
                            {
                                await SearchingProcessing(update);
                                break;
                            }
                    }
                }
                catch { }

            }

            #region threadsafe properties
            public State state 
            { 
                get 
                {
                    lock (stateLocker)
                    {
                        return _state;
                    }
                } 
                set 
                {
                    lock (stateLocker)
                    {
                        _state = value;
                    }
                } 
            }

            public Message LastBotMessage
            {
                get
                {
                    lock (lastMessageLocker)
                    {
                        return _lastMessage;
                    }
                }
                set
                {
                    lock (lastMessageLocker)
                    {
                        _lastMessage = value;
                    }
                }
            }

            public RequestDepth RequestDepth
            {
                get
                {
                    lock (depthLocker)
                    {
                        return _requestDepth;
                    }
                }
                set
                {
                    lock (depthLocker)
                    {
                        _requestDepth = value;
                    }
                }
            }

            public bool SearchInGroups
            {
                get
                {
                    lock (settingsLocker)
                    {
                        return _searchInGroups;
                    }
                }
                set
                {
                    lock (settingsLocker)
                    {
                        _searchInGroups = value;
                    }
                }
            }

            public bool SearchInChannels
            {
                get
                {
                    lock (settingsLocker)
                    {
                        return _searchInChannels;
                    }
                }
                set
                {
                    lock (settingsLocker)
                    {
                        _searchInChannels = value;
                    }
                }
            }

            public int Limit
            {
                get
                {
                    lock (settingsLocker)
                    {
                        return _limit;
                    }
                }
                set
                {
                    lock (settingsLocker)
                    {
                        _limit = value;
                    }
                }
            }
            #endregion

        }

        #endregion
    }
}
