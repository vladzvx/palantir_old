using Bot.Core.Models;
using Bot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Interfaces
{
    public interface IReadyProcessor<TBot> where TBot : IConfig, new()
    {
        ISendedItem Stop(Update update);
        public Task ProcessUpdate(Update update, Services.Bot.FSM<TBot> fsm);
    }
}
