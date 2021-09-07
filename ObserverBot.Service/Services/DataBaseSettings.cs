using Common;
using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObserverBot.Service.Services
{
    public class DataBaseSettingsObserver : IDataBaseSettings
    {
        public string ConnectionString1 { get => Options.GetConnectionString2(); }
        public int ConnectionPoolMaxSize { get => 5; }
        public int ConnectionPoolHotReserve { get => 1; }
    }
}
