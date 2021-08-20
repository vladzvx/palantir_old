namespace Common.Services.DataBase.Interfaces
{
    public interface ISettings
    {
        public int OrdersManagerCheckingPeriod => 3 * 60 * 1000;
    }
}
