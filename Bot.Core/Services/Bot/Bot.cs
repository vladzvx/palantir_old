using Bot.Core.Interfaces;
using Bot.Core.Models;
using System;
using System.Threading;
using Telegram.Bot;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public readonly TelegramBotClient botClient;
        private readonly CancellationToken cancellationToken;
        private readonly IBotSettings settings;
        public Bot(IBotSettings settings, CancellationTokenSource cancellationTokenSource)
        {
            botClient = new TelegramBotClient(settings.Token);
            TextMessage.defaultClient = botClient;
            cancellationToken = cancellationTokenSource.Token;

        }
        public void Start<TBot>() where TBot : IConfig, new()
        {
            botClient.StartReceiving<MessageHandler<TBot>>(cancellationToken);
            //botClient.StartReceiving<KeyboardHandler>(cancellationToken);
        }
        public void Dispose()
        {

        }
    }
}
