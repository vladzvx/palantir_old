using Bot.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.Tests.Models
{
    class TestMessage : ISendedItem
    {
        public static ConcurrentDictionary<long, int> statistic = new ConcurrentDictionary<long, int>();
        public static void Refresh()
        {
            statistic = new ConcurrentDictionary<long, int>();
        }

        private Channel<int> channel = Channel.CreateBounded<int>(1);
        public long ChatId { get; set; }

        public ChannelReader<int> Ready => channel.Reader;

        public async Task Send()
        {
            statistic.AddOrUpdate(ChatId, 1, (key, oldVal) => oldVal + 1);
            await channel.Writer.WriteAsync(0);
        }
    }
}
