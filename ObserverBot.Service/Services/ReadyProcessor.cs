using Bot.Core.Interfaces;
using Bot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class EmptyReadyProcessor : IReadyProcessor
    {
        public SearchBotConfig config => throw new NotImplementedException();

        public async Task ProcessUpdate(Update update, Func<object, Task> func)
        {
        }

        public void SetConfig(IConfig config)
        {

        }

        public ISendedItem Stop(Update update)
        {
            return null;
        }
    }
}
