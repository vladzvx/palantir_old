using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Interfaces.BotFSM
{
    public interface IFSMFactory<TBot> where TBot: IConfig
    {
        public Task<ISubFSM<TBot>> Get(Update update);

    }
}
