using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services
{
    public class LoadManager
    {
        public void AddValue(int size)
        {
            if (size > Options.SleepModeStartCount)
                Task.Delay(1000).Wait() ;
        }
    }
}
