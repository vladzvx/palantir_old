﻿using System;

namespace Common.Services.DataBase.Interfaces
{
    public interface IDataBaseSettings
    {
        public TimeSpan ReconnectionPause { get => new TimeSpan(0, 1, 0); }
        public double StartWritingInterval { get => 5000; }
        public double PoolingTimeout { get => 30000; }
        public string ConnectionString1 { get => Options.ConnectionString1;}
        public int CriticalQueueSize { get => 100000; }
        public int StartVectorizerCount { get => 1000; }
        public int TrasactionSize { get => 100000; }
        public int ConnectionPoolMaxSize { get => 1300; }
        public int ConnectionPoolHotReserve { get => 200; }
    }
}
