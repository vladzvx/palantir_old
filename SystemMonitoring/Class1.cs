using System;

namespace SystemMonitoring
{
    public class SystemMonitoring
    {
        System.Timers.Timer Timer = new System.Timers.Timer(1000);
        public struct Report
        {
            public double CPUUsage;
            public long FreeDisk;
            public long TotalMemory;
        }
        public Report GetReport()
        {
            return new Report() { FreeDisk = 0, CPUUsage = 0, TotalMemory = 0 };
        }
    }
}
