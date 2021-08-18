using Bot.Core.Enums;
using Bot.Core.Interfaces;
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
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public partial class Bot :IDisposable
    {
        private readonly protected TelegramBotClient botClient;
        private readonly CancellationToken cancellationToken;
        public Bot(IBotSettings settings, IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
        {
            botClient = new TelegramBotClient(settings.Token);
            cancellationToken = cancellationTokenSource.Token;
            if (FSM.serviceProvider ==null)
                FSM.serviceProvider = serviceProvider;
        }
        public void Start()
        {
            botClient.StartReceiving<MessageHandler>(cancellationToken);
        }
        public void Dispose()
        {

        }

        #region subclasses




        #endregion
    }
}
