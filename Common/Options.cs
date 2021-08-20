using System;
using System.IO;

namespace Common
{
    internal static class Options
    {
        internal static readonly string SettingsFilename = "settings.txt";
        internal static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? File.ReadAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Options.SettingsFilename));

        internal static TimeSpan OrderGenerationTimeSpan = -new TimeSpan(1, 10, 0);
        internal static double StartWritingInterval = 20000;
        internal static double OrderGenerationTimerPeriod = 25000;
        internal static double CollectorsSyncTimerPeriod = 15000;
        internal static int WriterTransactionSize = 100000;
        internal static int EntityWriterTrasactiobSize = 25000;
        internal static int ReconnectionRepeatCount = 5;
        internal static int SleepModeStartCount = 80000;
        internal static int Pause = 1000;
        internal static int SleepModeEndCount = SleepModeStartCount / 2;
        internal static TimeSpan ReconnerctionPause = new TimeSpan(0, 0, 5);
    }

}
