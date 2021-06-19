using Bot.Core.Interfaces;
using Bot.Core.Services;
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
        private readonly HttpClient httpClient;
        public Bot(IBotSettings settings, CancellationTokenSource cancellationTokenSource,HttpClient httpClient)
        {
            botClient = new TelegramBotClient(settings.Token);
            cancellationToken = cancellationTokenSource.Token;
            this.httpClient = httpClient;
            BotMessageHandler.httpClient = httpClient;
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
