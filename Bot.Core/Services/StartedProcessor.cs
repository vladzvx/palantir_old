using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class StartedProcessor : IStartedProcessor
    {
        public async Task Process(Update update, Func<object, Task> func)
        {
           
        }
    }
}
