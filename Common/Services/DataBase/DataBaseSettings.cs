using Common.Services.DataBase.Interfaces;
using System.Threading;

namespace Common.Services.DataBase
{
    public class DataBaseSettings : IDataBaseSettings
    {
        public CancellationToken Token { get; set; } = CancellationToken.None;
    }
}
