using Common.Services;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public class BotsEntryPoint : IHostedService
    {
        private readonly Bot mainBot;

        public BotsEntryPoint(Bot bot, IServiceProvider serviceProvider, DBWorker dBWorker)
        {
            mainBot = bot;
            if (Bot.FSM.serviceProvider == null)
            {
                Bot.FSM.serviceProvider = serviceProvider;
            }

            Bot.FSM.Factory.dBWorker = dBWorker;
            MessageHandler.writer = (ICommonWriter<Message>)serviceProvider.GetService(typeof(ICommonWriter<Message>));
            MessageHandler.asyncTaskExecutor = (AsyncTaskExecutor)serviceProvider.GetService(typeof(AsyncTaskExecutor));
            MessageHandler.searchState = (SearchState)serviceProvider.GetService(typeof(SearchState));
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
