using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair
{
    internal static class Options
    {
        internal static readonly string SettingsFilename = "settings.txt";
        internal static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? File.ReadAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Options.SettingsFilename));

        internal static TimeSpan OrderGenerationTimeSpan = - new TimeSpan(1, 10, 0);
        internal static double OrderGenerationTimerPeriod = 10000;
    }

}
