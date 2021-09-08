using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Interfaces
{
    public interface IStartedProcessor
    {
        public Task Process(Update update, Func<object, Task> func);
    }
}
