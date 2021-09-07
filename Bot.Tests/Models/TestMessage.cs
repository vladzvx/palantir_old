using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.Tests.Models
{
    class TestMessage : ISendedItem
    {
        private Channel<int> channel = Channel.CreateBounded<int>(1);
        public long ChatId { get; set; }

        public ChannelReader<int> Ready => channel.Reader;

        public async Task Send()
        {
            await channel.Writer.WriteAsync(0);
        }
    }
}
