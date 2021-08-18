using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Enums
{
    public enum BotState
    {
        Started = 0,
        ConfiguringDepth = 1,
        ConfiguringGroups = 4,
        ConfiguringChannel = 5,
        ConfiguringLimit = 6,
        Ready = 2,
        Searching = 3
    }
}
