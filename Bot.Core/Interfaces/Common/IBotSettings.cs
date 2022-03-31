using Bot.Core.Enums;
using Bot.Core.Models;
using System;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface IUserChecker
    {
        public Task<UserInfo> Check(long userId);
        public Task<UserInfo> Check(string username);
    }
    public interface IBotSettings
    {
        public string Token => "1109953036:AAGij9-o05wvYDcvSrXcUxB2465Ffv0xIF8";//"1397950961:AAFF-gJFp2Fu0pReMcN1zb9sx7_6xk494a0";// Environment.GetEnvironmentVariable("Token");

        public UserStatus BoundUserStatus => Enum.Parse<UserStatus>(Environment.GetEnvironmentVariable("BoundUserStatus"));

        public string MongoCnnStr => Environment.GetEnvironmentVariable("MongoDB1");
        //public string Mongo2CnnStr => Environment.GetEnvironmentVariable("MongoDB2");
    }
}
