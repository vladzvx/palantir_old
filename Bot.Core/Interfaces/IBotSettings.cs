using System;

namespace Bot.Core.Interfaces
{
    public interface IBotSettings
    {
        public string Token => Environment.GetEnvironmentVariable("Token");
    }
}
