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
        public static readonly SearchBotConfig Default = new SearchBotConfig()
        {
            SearchInChannels= true,
            SearchInGroups=false,
            RequestDepth = RequestDepth.Inf
        };
        public bool SearchInChannels { get; set; }
        public bool SearchInGroups { get; set; }
        public UserStatus Status { get; set; } = UserStatus.common;
        public PrivateChatState BotState { get; set; }
        public RequestDepth RequestDepth { get; set; }
        public bool Finished { get; set; }
    }
    public enum ConfiguringSubstates
    {
        Started,
        ConfiguringDepth,
        ConfiguringGroups,
        ConfiguringChannel
    }

    public interface IReadyProcessor
    {
        SearchBotConfig config {get;}
        ISendedItem Stop(Update update);
        public Task ProcessUpdate(Update update, Func<object, Task> func);
        public void SetConfig(IConfig config);
    }

    public interface IConfigurationProcessor
    {
        public Task<IConfig> ProcessUpdate(Update update, CancellationToken token);
    }
    public class ReadyProcessor : IReadyProcessor
    {
        public SearchBotConfig config { get; private set; }
        private readonly AsyncTaskExecutor asyncTaskExecutor;
        private readonly MessagesSender messagesSender;
        private CancellationTokenSource CancellationTokenSource;
        private Task SearchingTask;
        private readonly IServiceProvider serviceProvider;
        public ReadyProcessor(IServiceProvider serviceProvider,  AsyncTaskExecutor asyncTaskExecutor, MessagesSender messagesSender)
        {
            this.asyncTaskExecutor = asyncTaskExecutor;
            this.serviceProvider = serviceProvider;
            this.messagesSender = messagesSender;
        }
        public void SetConfig(IConfig config)
        {
            if (config is SearchBotConfig sbc)
            {
                this.config = (SearchBotConfig)config;
            }
            else
            {
                this.config = SearchBotConfig.Default;
            }

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
            SearchingTask = searchClient.Search(update.Message.From.Id, GetRequest(update), CancellationTokenSource.Token);
            Task searchFinalsTask = SearchingTask.ContinueWith(func);
            await Task.WhenAll(SearchingTask, searchFinalsTask);
            //return Task.CompletedTask;
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
        private SearchRequest GetRequest(Update update)
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
    }
    public class ConfigProcessor : IConfigurationProcessor
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
                        TextMessage mess = new TextMessage(null, update.Message.Chat.Id, "Выберите глубину поиска", Channel.CreateBounded<int>(1), keyboard: Constants.Keyboards.settingKeyboard);
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringDepth;
                        return current;
                    }
                case ConfiguringSubstates.ConfiguringDepth:
                    {
                        current.RequestDepth = ParseDepth(update);
                        ISendedItem mess = Bot.FSM.CreateOk(update, Constants.Keyboards.yesNoKeyboard, " Искать в группах?");
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringGroups;
                        return current;
                    }
                case ConfiguringSubstates.ConfiguringGroups:
                    {
                        current.SearchInGroups = update.Message.Text.ToLower() == "да";
                        ISendedItem mess = Bot.FSM.CreateOk(update, Constants.Keyboards.yesNoKeyboard, " Искать в каналах?");
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringChannel;
                        return current;
                    }
                case ConfiguringSubstates.ConfiguringChannel:
                    {
                        current.SearchInChannels = update.Message.Text.ToLower() == "да";
                        ISendedItem mess = Bot.FSM.CreateOk(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                        messagesSender.AddItem(mess);
                        current.Finished = true;
                        state = ConfiguringSubstates.Started;
                        return current;
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
            #region Properties and fields
            private readonly IConfigurationProcessor configProcessor;
            private readonly IReadyProcessor readyProcessor;
            private readonly AsyncTaskExecutor asyncTaskExecutor;
            private readonly DBWorker dBWorker;

            public static IServiceProvider serviceProvider;
            private readonly MessagesSender messagesSender;
            private readonly ICommonWriter<Message> writer;
            private readonly IBotSettings botSettings;
            public UserStatus UserStatus
            {
                get
                {
                    lock (stateLocker)
                    {
                        return _userStatus;
                    }
                }
                set
                {
                    lock (stateLocker)
                    {
                        _userStatus = value;
                    }
                }
            }

            private UserStatus _userStatus = UserStatus.common;

            public readonly ITelegramBotClient botClient;
            public readonly long chatId;
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

            #endregion

            public FSM(ITelegramBotClient botClient, long chatId, SearchBotConfig settings = null)
            {
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("IServiceProvider serviceProvider static field is null! Work impossible!");
                }

                this.botClient = botClient;
                this.chatId = chatId;

                messagesSender = (MessagesSender)serviceProvider.GetService(typeof(MessagesSender));
                writer = (ICommonWriter<Message>)serviceProvider.GetService(typeof(ICommonWriter<Message>));
                configProcessor = (IConfigurationProcessor)serviceProvider.GetService(typeof(IConfigurationProcessor));
                readyProcessor = (IReadyProcessor)serviceProvider.GetService(typeof(IReadyProcessor));
                asyncTaskExecutor = (AsyncTaskExecutor)serviceProvider.GetService(typeof(AsyncTaskExecutor));
                dBWorker = (DBWorker)serviceProvider.GetService(typeof(DBWorker));
                botSettings = (IBotSettings)serviceProvider.GetService(typeof(IBotSettings));
                if (settings != null)
                {
                    UserStatus = settings.Status;
                    BotState = settings.BotState;
                    readyProcessor.SetConfig(settings);
                }
                if (TextMessage.commonWriter == null)
                {
                    TextMessage.commonWriter = writer;
                }
            }
            public async Task Process(Update update, CancellationToken token)
            {
                if (this.UserStatus> botSettings.BoundUserStatus)
                {
                    messagesSender.AddItem(new TextMessage(null,update.Message.Chat.Id,"Пожалуйста, обратитесь к администратору для выдачи разрешения на продолжение работы.",null));
                    return;
                }
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
                                        readyProcessor.SetConfig(cfg);
                                        BotState = PrivateChatState.Ready;
                                        if (cfg is SearchBotConfig botConfig)
                                        {
                                            botConfig.BotState = BotState;
                                            await dBWorker.LogUser(update, CancellationToken.None, botConfig);
                                        }

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

            #region private
            private async Task StartedProcessing(Update update)
            {
                await TryStartConfiguring(update, true);
            }
            private async Task ReadyProcessing(Update update)
            {
                if (!await TryStartConfiguring(update))
                {
                    BotState = PrivateChatState.Busy;
                    asyncTaskExecutor.Add(readyProcessor.ProcessUpdate(update, (_) =>
                    {
                        BotState = PrivateChatState.Ready;
                        messagesSender.AddItem(new TextMessage(null, update.Message.Chat.Id, "Поиск завершен!", null, new ReplyKeyboardRemove()));
                        return Task.CompletedTask;
                    }));
                    messagesSender.AddItem(FSM.CreateOk(update, Bot.Constants.Keyboards.searchingKeyboard));
                }
            }
            private async Task BusyProcessing(Update update)
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
            private async Task<bool> TryStartConfiguring(Update update, bool force = false)
            {
                if (force||Constants.CallSettings.Contains(update.Message.Text.ToLower()))
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

            //public SearchBotConfig GetSettings()
            //{
            //    return new SearchBotConfig() {SearchInGroups = SearchInGroups, SearchInChannels = SearchInChannels, RequestDepth = RequestDepth };
            //}
            //public class Settings
            //{
            //    public UserStatus Status { get; set; } = UserStatus.common;
            //    public PrivateChatState BotState { get; set; } = PrivateChatState.Started;
            //    public RequestDepth Depth { get; set; } = RequestDepth.Month;
            //    public bool SearchInGroups { get; set; } = false;
            //    public bool SearchInChannels { get; set; } = true;

            //}

            #endregion
        }
    }
}
