using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Bot.Core.Interfaces
{
    public interface IBotSettings
    {
        public string Token => Environment.GetEnvironmentVariable("Token");
    }
}
