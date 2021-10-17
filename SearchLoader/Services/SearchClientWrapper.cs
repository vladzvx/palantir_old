using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SearchLoader.Services
{
    public class SearchClientWrapper
    {
        public static Channel<TimeSpan> channel = Channel.CreateUnbounded<TimeSpan>();
        public SearchClientWrapper()
        {

        }
    }
}
