﻿using Bot.Core.Interfaces;
using Bot.Core.Models;
using System;
using System.Threading;
using Telegram.Bot;

namespace Bot.Core.Services
{
    public partial class Bot : IDisposable
    {
        private readonly protected TelegramBotClient botClient;
        private readonly CancellationToken cancellationToken;
        public Bot(IBotSettings settings, CancellationTokenSource cancellationTokenSource)
        {
            botClient = new TelegramBotClient(settings.Token);
            TextMessage.defaultClient = botClient;
            cancellationToken = cancellationTokenSource.Token;

        }
        public void Start()
        {
            botClient.StartReceiving<MessageHandler>(cancellationToken);
            //botClient.StartReceiving<KeyboardHandler>(cancellationToken);
        }
        public void Dispose()
        {

        }

        #region subclasses




        #endregion
    }
}