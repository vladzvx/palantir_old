using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GateKeeper.Service.Services
{
    public class DBSettings : IDataBaseSettings
    {
        public CancellationToken Token { get; set; } = CancellationToken.None;
        public int ConnectionPoolHotReserve { get => 20; }
    }
}
