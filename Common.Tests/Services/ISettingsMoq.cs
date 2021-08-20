using Common.Services.DataBase.Interfaces;

namespace Common.Tests.Services
{
    public class ISettingsMoq : ISettings
    {
        public int OrdersManagerCheckingPeriod => 1 * 1 * 500;
    }
}
