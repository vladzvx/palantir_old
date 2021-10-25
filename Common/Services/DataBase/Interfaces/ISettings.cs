using System;

namespace Common.Services.DataBase.Interfaces
{
    public interface ISettings
    {
        public int OrdersManagerCheckingPeriod => 3 * 60 * 1000;
        public TimeSpan UpdatesCheckingPeriod => new TimeSpan(0, 30, 0);
    }
}
