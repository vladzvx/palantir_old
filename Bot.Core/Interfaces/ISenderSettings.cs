using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface ISenderSettings
    {
        public int BufferSize => 30;
        public TimeSpan MainPeriod => new TimeSpan(0,0,1);

        public int MaxQueueSize => 100000;
    }
}
