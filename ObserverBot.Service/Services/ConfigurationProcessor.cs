using Bot.Core.Services;
using ObserverBot.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class ConfigurationProcessor : IConfigurationProcessor
    {
        public async Task<IConfig> ProcessUpdate(Update update, CancellationToken token)
        {
            return new Cfg();
        }
    }
}
