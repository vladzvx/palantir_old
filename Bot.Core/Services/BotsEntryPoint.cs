using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class BotsEntryPoint : IHostedService
    {
        private readonly Bot mainBot;
        

        public BotsEntryPoint(Bot bot)
        {
            mainBot = bot;
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
