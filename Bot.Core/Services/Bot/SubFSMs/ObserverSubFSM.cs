using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class EmptySubFSM<TBot> : ISubFSM<TBot> where TBot:IConfig
    {
        public bool IsEmpty => true;

        public async Task<bool> ProcessUpdate(Update update, TBot parentFSM, CancellationToken token)
        {
            return true;
        }
    }
}
