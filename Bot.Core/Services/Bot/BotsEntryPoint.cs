using Bot.Core.Interfaces;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Message = Telegram.Bot.Types.Message;

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

            Bot.FSM<TBot>.Factory.dBWorker = dBWorker;
            MessageHandler<TBot>.writer = (ICommonWriter<Message>)serviceProvider.GetService(typeof(ICommonWriter<Message>));
            MessageHandler<TBot>.asyncTaskExecutor = (AsyncTaskExecutor)serviceProvider.GetService(typeof(AsyncTaskExecutor));
            MessageHandler<TBot>.searchState = (SearchState)serviceProvider.GetService(typeof(SearchState));
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            mainBot.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            mainBot.Dispose();
            return Task.CompletedTask;
        }
    }
}
