using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationProvider.Services
{
    public class DataBaseSetting : IDataBaseSettings
    {
        public CancellationToken Token { get; set; } = CancellationToken.None;
        public TimeSpan ReconnectionPause { get => new TimeSpan(0, 1, 0); }
        public TimeSpan ConnectionLifetime { get => new TimeSpan(0, 10, 0); }
        public int ConnectionPoolMaxSize { get => 70; }
        public int ConnectionPoolHotReserve { get => 5; }
    }
}
