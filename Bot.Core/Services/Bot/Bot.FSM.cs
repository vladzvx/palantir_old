using Bot.Core.Enums;
using Bot.Core.Interfaces;
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
using static Bot.Core.Services.Bot;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public interface IConfig
    {
        public bool Finished { get; }
    }

    public class SearchBotConfig : IConfig
    {
        public RequestDepth RequestDepth;
        public bool Finished { get; set; }
    }
    public enum ConfiguringSubstates
    {
        Started,
        ConfiguringDepth,
        ConfiguringGroups,
        ConfiguringChannel
    }

    public class ReadyProcessor
    {
        private readonly AsyncTaskExecutor asyncTaskExecutor;
        private CancellationTokenSource CancellationTokenSource;
        private Task SearchingTask;
        private readonly IServiceProvider serviceProvider;
        private readonly MessagesSender messagesSender;
        public ReadyProcessor(IServiceProvider serviceProvider, MessagesSender messagesSender, AsyncTaskExecutor asyncTaskExecutor)
        {
            this.asyncTaskExecutor = asyncTaskExecutor;
            this.serviceProvider = serviceProvider;
            this.messagesSender = messagesSender;
        }

        public ISendedItem Stop(Update update)
        {
            CancellationTokenSource.Cancel();
            return Bot.FSM.CreateOk(update, new ReplyKeyboardRemove(), " Вы можете искать снова.");
        }
        public async Task ProcessUpdate(Update update,Func<object,Task> func)
        {
            CancellationTokenSource = new CancellationTokenSource();
            SearchReciever searchClient = (SearchReciever)serviceProvider.GetService(typeof(SearchReciever));
            SearchingTask = searchClient.Search(update.Message.From.Id, null, CancellationTokenSource.Token);
            Task searchFinalsTask = SearchingTask.ContinueWith(func);
            asyncTaskExecutor.Add(Task.WhenAll(SearchingTask, searchFinalsTask));
        }
    }
    public class ConfigProcessor
    {
        private ConfiguringSubstates state;
        private readonly MessagesSender messagesSender;


        private SearchBotConfig current = new SearchBotConfig();
        public ConfigProcessor(MessagesSender messagesSender)
        {
            this.messagesSender = messagesSender;
        }
        public async Task<IConfig> ProcessUpdate(Update update, CancellationToken token)
        {
            switch (state)
            {
                case ConfiguringSubstates.Started:
                    {
                        current = new SearchBotConfig();

                        break;
                    }
                case ConfiguringSubstates.ConfiguringDepth:
                    {

                        current.RequestDepth = ParseDepth(update);
                        Bot.FSM.CreateOk(update, Constants.Keyboards.yesNoKeyboard, " Искать в группах?");
                        //await Ok(update, Constants.Keyboards.yesNoKeyboard, );
                        //BotState = BotState.ConfiguringGroups;
                        return current;
                        break;
                    }
                case ConfiguringSubstates.ConfiguringGroups:
                    {
                        //SearchInGroups = update.Message.Text.ToLower() == "да";
                        //await Ok(update, Constants.Keyboards.yesNoKeyboard, " Искать в каналах?");
                        //BotState = BotState.ConfiguringChannel;
                        return current;
                        break;
                    }
                case ConfiguringSubstates.ConfiguringChannel:
                    {
                        //SearchInChannels = update.Message.Text.ToLower() == "да";
                        //await Ok(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                        //BotState = PrivateChatState.Ready;
                        //await dBWorker.LogUser(update, token, GetSettings());
                        current.Finished = true;
                        return current;
                        break;
                    }
                default:
                    { 
                        return current;
                    }
            }
        }


        private RequestDepth ParseDepth(Update update)
        {
            string tmp = update.Message.Text.ToLower();
            if (Constants.Day.Contains(tmp))
            {
                return RequestDepth.Day;
            }
            else if (Constants.Week.Contains(tmp))
            {
                return RequestDepth.Week;
            }
            else if (Constants.Month.Contains(tmp))
            {
                return RequestDepth.Month;
            }
            else if (Constants.Year.Contains(tmp))
            {
                return RequestDepth.Year;
            }
            else
            {
                return RequestDepth.Inf;
            }

        }
    }

    public partial class Bot
    {
        public partial class FSM
        {
            private readonly ConfigProcessor configProcessor;
            private readonly ReadyProcessor readyProcessor;
            public static IServiceProvider serviceProvider;

            private readonly AsyncTaskExecutor asyncTaskExecutor;
            private readonly MessagesSender messagesSender;
            private readonly DBWorker dBWorker;
            private readonly ICommonWriter<Message> writer;


            public readonly ITelegramBotClient botClient;
            public readonly long chatId;



            #region Properties

            private readonly object stateLocker = new object();
            private PrivateChatState _state;
            public PrivateChatState BotState
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
                configProcessor = (ConfigProcessor)serviceProvider.GetService(typeof(ConfigProcessor));
                readyProcessor = (ReadyProcessor)serviceProvider.GetService(typeof(ReadyProcessor));
                if (TextMessage.commonWriter == null)
                {
                    TextMessage.commonWriter = writer;
                }
            }


            #region common
            public async Task Process(Update update, CancellationToken token)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                {
                    writer.PutData(update.Message);
                    try
                    {
                        switch (BotState)
                        {
                            case PrivateChatState.Started:
                                {
                                    await StartedProcessing(update);
                                    break;
                                }
                            case PrivateChatState.Configuring:
                                {
                                    IConfig cfg = await configProcessor.ProcessUpdate(update, token);
                                    if (cfg.Finished)
                                    {
                                        BotState = PrivateChatState.Ready;
                                    }
                                    break;
                                }
                            case PrivateChatState.Ready:
                                {
                                    await ReadyProcessing(update);
                                    break;
                                }
                            case PrivateChatState.Busy:
                                {
                                    await BusyProcessing(update);
                                    break;
                                }
                        }
                    }
                    catch { }
                }
            }

            private async Task StartedProcessing(Update update)
            {
                await TryStartConfiguring(update);
            }

            private async Task ReadyProcessing(Update update)
            {
                if (!await TryStartConfiguring(update))
                {
                    BotState = PrivateChatState.Busy;
                    await readyProcessor.ProcessUpdate(update, (_)=> 
                    {
                        BotState = PrivateChatState.Ready;
                        return Task.CompletedTask;
                    });
                }
            }

            public async Task BusyProcessing(Update update)
            {
                if (Constants.Cancells.Contains(update.Message.Text.ToLower()))
                {
                    ISendedItem reply = readyProcessor.Stop(update);
                    messagesSender.AddItem(reply);
                    BotState = PrivateChatState.Ready;
                }
                else
                {
                    messagesSender.AddItem(CreateImBusy(update));
                }
            }

            private async Task<bool> TryStartConfiguring(Update update)
            {
                if (Constants.CallSettings.Contains(update.Message.Text.ToLower()))
                {
                    BotState = PrivateChatState.Configuring;
                    await configProcessor.ProcessUpdate(update, CancellationToken.None);
                    return true;
                }
                return false;
            }

            public static ISendedItem CreateImBusy(Update update)
            {
                return new TextMessage(null, update.Message.Chat.Id, Constants.BusyMessage, Channel.CreateBounded<int>(1), keyboard: Constants.Keyboards.searchingKeyboard);
            }

            public static ISendedItem CreateOk(Update update, IReplyMarkup keyboard = null, string additionalMessage = "")
            {
                return new TextMessage(null, update.Message.Chat.Id, Constants.OkMessage + additionalMessage, Channel.CreateBounded<int>(1), keyboard, replyToMessageId: update.Message.MessageId);
            }

            public Settings GetSettings()
            {
                return new Settings() { BotState = BotState, SearchInGroups = SearchInGroups, SearchInChannels = SearchInChannels, Depth = RequestDepth };
            }
            public class Settings
            {
                public UserStatus Status { get; set; } = UserStatus.common;
                public PrivateChatState BotState { get; set; } = PrivateChatState.Started;
                public RequestDepth Depth { get; set; } = RequestDepth.Month;
                public bool SearchInGroups { get; set; } = false;
                public bool SearchInChannels { get; set; } = true;

            }

            #endregion


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



        }
    }
}
