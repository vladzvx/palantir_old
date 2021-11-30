using Bot.Core.Interfaces;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class BotsEntryPoint<TBot> : IHostedService where TBot:IConfig, new()
    {
        private readonly Bot mainBot;

        public BotsEntryPoint(Bot bot, IServiceProvider serviceProvider, IDataStorage<TBot> dBWorker)
        {
            mainBot = bot;
            if (Bot.FSM<TBot>.serviceProvider == null)
            {
                Bot.FSM<TBot>.serviceProvider = serviceProvider;
            }
            var q = dBWorker.GetType();
            Bot.FSM<TBot>.Factory.dBWorker = dBWorker;
            MessageHandler<TBot>.writer = (MongoWriter)serviceProvider.GetService(typeof(MongoWriter));
            if (bot.botClient != null && bot.botClient.BotId.HasValue)
                MessageHandler<TBot>.writer.Start(bot.botClient.BotId.Value);
            MessageHandler<TBot>.asyncTaskExecutor = (AsyncTaskExecutor)serviceProvider.GetService(typeof(AsyncTaskExecutor));
            MessageHandler<TBot>.searchState = (SearchState<TBot>)serviceProvider.GetService(typeof(SearchState<TBot>));
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            mainBot.Start<TBot>();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            mainBot.Dispose();
            return Task.CompletedTask;
        }
    }
}
