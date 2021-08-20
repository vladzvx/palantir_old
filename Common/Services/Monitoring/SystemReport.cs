using System.Diagnostics;
using System.IO;

namespace Common.Services
{
    public class SystemReport
    {
        public long MemoryUsed;
        public long FreeDisk;

        public SystemReport()
        {
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
            FreeDisk = Disk / 1024 / 1024 / 1024;
        }
    }
}
