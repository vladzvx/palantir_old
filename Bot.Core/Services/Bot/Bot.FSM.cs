using Bot.Core.Enums;
using Bot.Core.Models;
using Common;
using Common.Services;
using Common.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public partial class FSM
        {
            public static IServiceProvider serviceProvider;


            private readonly AsyncTaskExecutor asyncTaskExecutor;
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

            #endregion

            public FSM(ITelegramBotClient botClient, long chatId, Settings settings = null)
            {
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("IServiceProvider serviceProvider static field is null! Work impossible!");
                }

                this.botClient = botClient;
                this.chatId = chatId;
                if (settings != null)
                {
                    this.SearchInChannels = settings.SearchInChannels;
                    this.SearchInGroups = settings.SearchInGroups;
                    this.BotState = settings.BotState;
                    this.RequestDepth = settings.Depth;
                }
                messagesSender = (MessagesSender)serviceProvider.GetService(typeof(MessagesSender));
                dBWorker = (DBWorker)serviceProvider.GetService(typeof(DBWorker));
                writer = (ICommonWriter<Message>)serviceProvider.GetService(typeof(ICommonWriter<Message>));
                asyncTaskExecutor = (AsyncTaskExecutor)serviceProvider.GetService(typeof(AsyncTaskExecutor));
                if (TextMessage.commonWriter == null)
                {
                    TextMessage.commonWriter = writer;
                }
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
                    Channel<int> channel = Channel.CreateBounded<int>(1);
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
            private async Task Ok(Update update, IReplyMarkup keyboard = null, string additionalMessage = "")
            {
                Channel<int> channel = Channel.CreateBounded<int>(1);
                TextMessage textMessage = new TextMessage(botClient, chatId, "Принято! " + additionalMessage, channel, keyboard, replyToMessageId: update.Message.MessageId);
                messagesSender.AddItem(textMessage);
                await channel.Reader.ReadAsync();
            }
            private async Task ImBusy()
            {
                Channel<int> channel = Channel.CreateBounded<int>(1);
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
                    SearchReciever searchClient = (SearchReciever)serviceProvider.GetService(typeof(SearchReciever));
                    SearchingTask = searchClient.Search(update.Message.From.Id, GetRequest(update), CancellationTokenSource.Token);
                    Task searchFinalsTask = SearchingTask.ContinueWith((state) =>
                    {
                        TextMessage textMessage = new TextMessage(botClient, chatId, "Поиск завершен!", null, new ReplyKeyboardRemove());
                        messagesSender.AddItem(textMessage);
                        BotState = BotState.Ready;
                    });
                    await SearchingTask;
                    await searchFinalsTask;
                }
            }
            public async Task ProcessUpdate(Update update, CancellationToken token)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    writer.PutData(update.Message);
                    try
                    {
                        switch (BotState)
                        {
                            case BotState.Started:
                                {
                                    if (await TryEnterSearchingMode(update))
                                    {
                                        Channel<int> channel = Channel.CreateBounded<int>(1);
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
                                    await Ok(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                                    BotState = BotState.Ready;
                                    await dBWorker.LogUser(update, token, GetSettings());
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
                return new Settings() { BotState = BotState, SearchInGroups = SearchInGroups, SearchInChannels = SearchInChannels, Depth = RequestDepth };
            }
            public class Settings
            {
                public UserStatus Status { get; set; } = UserStatus.common;
                public BotState BotState { get; set; } = BotState.Started;
                public RequestDepth Depth { get; set; } = RequestDepth.Month;
                public bool SearchInGroups { get; set; } = false;
                public bool SearchInChannels { get; set; } = true;
            }
        }
    }
}
