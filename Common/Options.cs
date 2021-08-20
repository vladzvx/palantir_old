using System;
using System.IO;

namespace Common
{
    internal static class Options
    {
        internal static string GetConnectionString()
        {
            try
            {
                string pwd = Environment.GetEnvironmentVariable("db_pwd");
                string db = Environment.GetEnvironmentVariable("db");
                string user = Environment.GetEnvironmentVariable("user");
                string result = string.Format("User ID={2};Password={0};Host=localhost;Port=5432;Database={1};Pooling=true;Timeout=30;CommandTimeout=0;", pwd, db, user);

                return pwd != null && db != null && user != null ? result : null;
            }
            catch 
            {
                return null;
            }

        }

        internal static readonly string SettingsFilename = "settings.txt";
        internal static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? GetConnectionString()??(File.ReadAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Options.SettingsFilename)));

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
