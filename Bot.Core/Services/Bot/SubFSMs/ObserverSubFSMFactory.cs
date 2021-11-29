using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class EmptySubFSMFactory<TBot> : IFSMFactory<TBot> where TBot:IConfig
    {
        public async Task<ISubFSM<TBot>> Get(Update update)
        {
            return new EmptySubFSM<TBot>();
        }
    }
}
