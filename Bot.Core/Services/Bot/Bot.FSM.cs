using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Bot.Core.Services;
using Common;
using Common.Services.DataBase;
using Common.Services.gRPC;
using Common.Services.Interfaces;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public class FSM
        {
            public static IServiceProvider serviceProvider;


            private readonly MessagesSender messagesSender;
            private readonly DBWorker dBWorker;
            private readonly ICommonWriter<Message> writer;


            public readonly ITelegramBotClient botClient;
            public readonly long chatId;

            private CancellationTokenSource CancellationTokenSource;
            private Task SearchingTask;

            #region Properties

            private readonly object stateLocker = new object();
            private BotState _state;
            public BotState BotState
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

            private readonly object lastMessageLocker = new object();
            private Message _lastMessage;
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


            private readonly object depthLocker = new object();
            private RequestDepth _requestDepth;
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

            private readonly object settingsLocker = new object();
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
            private bool _searchInGroups = true;
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
            private bool _searchInChannels = true;
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
            private int _limit = 150;
            #endregion

            public FSM(ITelegramBotClient botClient, long chatId, Settings settings=null)
            {
                if (serviceProvider == null) throw new ArgumentNullException("IServiceProvider serviceProvider static field is null! Work impossible!");
                this.botClient = botClient;
                this.chatId = chatId;
                if (settings != null)
                {
                    this.SearchInChannels = settings.SearchInChannels;
                    this.SearchInGroups = settings.SearchInGroups;
                    this.BotState = settings.BotState;
                }
                messagesSender = (MessagesSender)serviceProvider.GetService(typeof(MessagesSender));
                dBWorker = (DBWorker)serviceProvider.GetService(typeof(DBWorker));
                writer = (ICommonWriter<Message>)serviceProvider.GetService(typeof(ICommonWriter<Message>));
                TextMessage.commonWriter = writer;
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
                    IsChannel = SearchInChannels,
                    IsGroup = SearchInGroups,
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
                            start = DateTime.UtcNow.Date.AddDays(-365);
                            break;
                        }
                }
                request.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(start);
                request.EndTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(end);
                return request;
            }
            private void ParseDepth(Update update)
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
            private async Task<bool> TryEnterSearchingMode(Update update)
            {
                if (Constants.CallSettings.Contains(update.Message.Text.ToLower()))
                {
                    BotState = BotState.ConfiguringDepth;
                    Channel<bool> channel = Channel.CreateBounded<bool>(1);
                    TextMessage textMessage = new TextMessage(botClient, chatId, "Выберите глубину поиска", channel, keyboard: Constants.Keyboards.settingKeyboard);
                    messagesSender.AddItem(textMessage);
                    await channel.Reader.ReadAsync();
                    return false;
                }
                else
                {
                    return true;
                }
            }

            private bool CheckStatus(UserStatus status)
            {
                return status != UserStatus.banned;
            }
            private async Task Ok(Update update, IReplyMarkup keyboard = null, string additionalMessage = "")
            {
                Channel<bool> channel = Channel.CreateBounded<bool>(1);
                TextMessage textMessage = new TextMessage(botClient, chatId, "Принято! " + additionalMessage, channel, keyboard, replyToMessageId: update.Message.MessageId);
                messagesSender.AddItem(textMessage);
                await channel.Reader.ReadAsync();
            }

            #region searching processing
            private async Task ImBusy()
            {
                Channel<bool> channel = Channel.CreateBounded<bool>(1);
                TextMessage textMessage = new TextMessage(botClient, chatId, "Идет поиск, но вы можете отменить его.", channel, keyboard: Constants.Keyboards.searchingKeyboard);
                messagesSender.AddItem(textMessage);
                await channel.Reader.ReadAsync();
            }

            private async Task SearchingProcessing(Update update)
            {
                if (Constants.Cancells.Contains(update.Message.Text.ToLower()))
                {
                    CancellationTokenSource.Cancel();
                    await Ok(update, new ReplyKeyboardRemove(), " Вы можете искать снова.");
                    BotState = BotState.Ready;
                }
                else
                {
                    await ImBusy();
                }
            }


            private async Task ReadyProcessing(Update update)
            {
                if (await TryEnterSearchingMode(update))
                {
                    BotState = BotState.Searching;
                    CancellationTokenSource = new CancellationTokenSource();





                    SearchClient searchClient = (SearchClient)serviceProvider.GetService(typeof(SearchClient));
                    SearchingTask = searchClient.Search(GetRequest(update), CancellationTokenSource.Token);
                    MessagesSenderOld messagesSender = new MessagesSenderOld(botClient, update.Message.Chat.Id, CancellationTokenSource.Token, Limit);
                    Task SendingTask = Task.Factory.StartNew(async () =>
                    {
                        int count = 0;
                        CancellationToken token = CancellationTokenSource.Token;
                        while (!searchClient.searchResultReciever.IsComplited && !token.IsCancellationRequested)
                        {
                            await Task.Delay(200);
                            while (searchClient.searchResultReciever.TryDequeueResult(out SearchResult res) && !token.IsCancellationRequested)
                            {
                                messagesSender.Send(res);
                                count++;
                            }
                        }
                        if (count == 0)
                        {
                            LastBotMessage = await botClient.SendTextMessageAsync(chatId, "Ничего не найдено, попробуйте другой запрос.", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: CancellationTokenSource.Token);
                            CancellationTokenSource.Cancel();
                            BotState = BotState.Ready;
                        }
                    });
                    Task.Factory.StartNew(() =>
                    {
                        Task.WaitAll(SendingTask, SearchingTask);
                        if (messagesSender.sendingTask != null)
                        {
                            messagesSender.sendingTask.ContinueWith((prevState) =>
                            {
                                BotState = BotState.Ready;
                                LastBotMessage = botClient.SendTextMessageAsync(chatId, "Поиск завершен!", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: CancellationTokenSource.Token).Result;
                            });
                        }

                    });
                }
            }
            #endregion
            public async Task ProcessUpdate(Update update)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    writer.PutData(update.Message);
                    UserStatus status = await dBWorker.LogUser(update, CancellationToken.None);
                    if (!CheckStatus(status))
                    {
                        Channel<bool> channel = Channel.CreateBounded<bool>(1);
                        TextMessage textMessage = new TextMessage(botClient, chatId, "Вас приветствует бот-поисковик по телеграму! Вы не входите в число пользователей, пожалуйста, свяжитесь с владельцами бота!", channel);
                        messagesSender.AddItem(textMessage);
                        await channel.Reader.ReadAsync();
                    }
                    try
                    {
                        switch (BotState)
                        {
                            case BotState.Started:
                                {
                                    if (await TryEnterSearchingMode(update))
                                    {
                                        Channel<bool> channel = Channel.CreateBounded<bool>(1);
                                        TextMessage textMessage = new TextMessage(botClient, chatId, "Вас приветствует бот-поисковик по телеграму! Здесь вы можете найти пост или комментарий по ключевым словам. Для начала работы настройте бота, набрав команду /settings", channel);
                                        messagesSender.AddItem(textMessage);
                                        await channel.Reader.ReadAsync();
                                    }
                                    break;
                                }
                            case BotState.ConfiguringDepth:
                                {
                                    ParseDepth(update);
                                    await Ok(update, Constants.Keyboards.yesNoKeyboard, " Искать в группах?");
                                    BotState = BotState.ConfiguringGroups;
                                    break;
                                }
                            case BotState.ConfiguringGroups:
                                {
                                    SearchInGroups = update.Message.Text.ToLower() == "да";
                                    await Ok(update, Constants.Keyboards.yesNoKeyboard, " Искать в каналах?");
                                    BotState = BotState.ConfiguringChannel;
                                    break;
                                }
                            case BotState.ConfiguringChannel:
                                {
                                    SearchInChannels = update.Message.Text.ToLower() == "да";
                                    await Ok(update, Constants.Keyboards.limtKeyboard, " Выберете предельное число результатов (по умолчанию - 150)");
                                    BotState = BotState.ConfiguringLimit;
                                    break;
                                }
                            case BotState.ConfiguringLimit:
                                {
                                    if (int.TryParse(update.Message.Text.ToLower(), out int res))
                                    {
                                        await Ok(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                                        BotState = BotState.Ready;
                                        Limit = res;
                                    }
                                    break;
                                }
                            case BotState.Ready:
                                {
                                    await Ok(update, Constants.Keyboards.searchingKeyboard);
                                    await ReadyProcessing(update);
                                    break;
                                }
                            case BotState.Searching:
                                {
                                    await SearchingProcessing(update);
                                    break;
                                }
                        }
                    }
                    catch { }
                }


            }

            public Settings GetSettings()
            {
                return new Settings() { BotState = BotState, SearchInGroups=SearchInGroups, SearchInChannels= SearchInChannels };
            }

            public class Settings
            {
                public BotState BotState { get; set; }
                public bool SearchInGroups { get; set; }
                public bool SearchInChannels { get; set; }
            }
        }
    }
}
