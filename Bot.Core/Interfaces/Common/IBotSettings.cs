using Bot.Core.Enums;
using System;

namespace Bot.Core.Interfaces
{
    public interface IBotSettings
    {
        public string Token => Environment.GetEnvironmentVariable("Token");

        public UserStatus BoundUserStatus => Enum.Parse<UserStatus>(Environment.GetEnvironmentVariable("BoundUserStatus"));

        public string MongoCnnStr => Environment.GetEnvironmentVariable("MongoDB1");
        //public string Mongo2CnnStr => Environment.GetEnvironmentVariable("MongoDB2");
    }
}
