using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static Bot.Core.Services.Bot;

namespace Bot.Core.Interfaces
{
    public interface ISubFSM<TBot> where TBot : IConfig
    {
        public bool IsEmpty { get; }
        public Task<bool> ProcessUpdate(Update update, TBot parentFSM, CancellationToken token);
    }
}
