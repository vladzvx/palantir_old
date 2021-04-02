using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace SystemMonitoring
{
    public class SystemMonitoring
    {
        internal System.Timers.Timer MonitoringTimer;
        internal System.Timers.Timer EraseTimer;
        private DateTime LastMonitoringTime = DateTime.MinValue;
        private object locker = new object();
        public SystemMonitoring(double monitoringPeriod, double erasePeriod=300000)
        {
            if (erasePeriod < monitoringPeriod) throw new ArgumentException("Erasing period smaller then monitoring!");
            MonitoringTimer = new Timer(monitoringPeriod);
            EraseTimer = new Timer(erasePeriod);
            MonitoringTimer.Elapsed += Monitoring;
            EraseTimer.Elapsed += Erasing;
            MonitoringTimer.AutoReset = true;
            EraseTimer.AutoReset = true;

        }
        public void Start()
        {
            MonitoringTimer.Start();
            EraseTimer.Start();
        }
        public struct Report
        {
            public double CPUUsage;
            public long FreeDisk;
            public long TotalMemory;
        }
        public Report GetReport()
        {
            lock (locker)
            {
                if (LastMonitoringTime == DateTime.MinValue) throw new Exception("No monitorings started yet!");
            }
            
            long Memory = 0;
            long Disk = 0;
            foreach (Process proc in Process.GetProcesses())
            {
                Memory += proc.WorkingSet64;
            }
            Memory = Memory / 1024 / 1024;

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    Disk += d.TotalFreeSpace;
                }
            }
            Disk = Disk / 1024 / 1024 / 1024;

            return new Report() { FreeDisk = Disk, CPUUsage = 0, TotalMemory = Memory };
        }

        internal void Monitoring(object sender, ElapsedEventArgs args)
        {

            foreach (Process proc in Process.GetProcesses())
            {
                var d  =  proc.TotalProcessorTime;
                try
                {
                    Console.WriteLine(proc.ProcessName + "; " + proc.StartTime.ToString());
                }
                catch { }
               
            }
            lock (locker)
            {
                LastMonitoringTime = DateTime.UtcNow;
            }
        }

        internal void Erasing(object sender, ElapsedEventArgs args)
        {

        }
    }
}
