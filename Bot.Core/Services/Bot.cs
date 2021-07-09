using Bot.Core.Interfaces;
using Bot.Core.Services;
using Common.Services.Interfaces;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;

namespace Bot.Core.Services
{
    public class Bot :IDisposable
    {
        private readonly protected TelegramBotClient botClient;
        private readonly CancellationToken cancellationToken;
        public Bot(IBotSettings settings, IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
        {
            botClient = new TelegramBotClient(settings.Token);
            cancellationToken = cancellationTokenSource.Token;
            BotMessageHandler.serviceProvider = serviceProvider;
        }

        public void Start()
        {
            botClient.StartReceiving<BotMessageHandler>(cancellationToken);
        }
        public void Dispose()
        {
            botClient.StartReceiving<BotMessageHandler>();
        }
    }
}
