using System;
using System.IO;

namespace Common
{
    public static class Options
    {
        internal static string GetConnectionString()
        {
            try
            {
                string pwd = Environment.GetEnvironmentVariable("db_pwd");
                string db = Environment.GetEnvironmentVariable("db");
                string user = Environment.GetEnvironmentVariable("user");
                string host = Environment.GetEnvironmentVariable("db_host");
                string result = string.Format("User ID={2};Password={0};Host={3};Port=5432;Database={1};Pooling=true;Timeout=30;CommandTimeout=0;", pwd, db, user, host);

                return pwd != null && db != null && user != null ? result : null;
            }
            catch 
            {
                return null;
            }

        }

        public static string GetConnectionString2()
        {
            try
            {
                string pwd = Environment.GetEnvironmentVariable("db_pwd");
                string db = Environment.GetEnvironmentVariable("db2");
                string user = Environment.GetEnvironmentVariable("user");
                string host = Environment.GetEnvironmentVariable("db_host");
                string result = string.Format("User ID={2};Password={0};Host={3};Port=5432;Database={1};Pooling=true;Timeout=30;CommandTimeout=0;", pwd, db, user, host);

                return pwd != null && db != null && user != null ? result : null;
            }
            catch
            {
                return null;
            }

        }

        internal static readonly string SettingsFilename = "settings.txt";
        internal static readonly string ConnectionString1 = Environment.GetEnvironmentVariable("ConnectionString") ?? GetConnectionString()??(File.ReadAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Options.SettingsFilename)));

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
