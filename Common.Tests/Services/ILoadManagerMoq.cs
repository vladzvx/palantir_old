using Common.Services.DataBase.Interfaces;

namespace Common.Tests.Services
{
    public class ILoadManagerMoq : ILoadManager
    {
        public bool CheckPauseNecessity()
        {
            return false;
        }
    }
}
