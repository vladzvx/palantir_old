using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class ObserverSubFSMFactory : IFSMFactory<Bot.Core.Models.ObserverBot>
    {
        public async Task<ISubFSM<Bot.Core.Models.ObserverBot>> Get(Update update)
        {
            return new ObserverSubFSM();
        }
    }
}
