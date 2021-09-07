using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Tests.Implementations
{
    class TestSettings : ISenderSettings
    {
        public int BufferSize => 30;
        public TimeSpan MainPeriod => new TimeSpan(0, 0, 0, 0, 200);
        public int MaxQueueSize => 100000;
    }
}
