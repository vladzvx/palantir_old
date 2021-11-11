using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class ObserverSubFSM : ISubFSM<Bot.Core.Models.ObserverBot>
    {
        public bool IsEmpty => true;

        public async Task<bool> ProcessUpdate(Update update, Bot.Core.Models.ObserverBot parentFSM, CancellationToken token)
        {
            return true;
        }
    }
}
