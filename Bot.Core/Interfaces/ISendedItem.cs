using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface ISendedItem
    {
        public Task Send();
        public long ChatId { get;}

        public ChannelReader<bool> Ready { get; }

    }
}
