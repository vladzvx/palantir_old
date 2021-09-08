using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class ObsStartedProcessor : IStartedProcessor
    {
        public async Task Process(Update update, Func<object, Task> func)
        {
            await func(null);
        }
    }
}
