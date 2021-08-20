using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface ISendedItem
    {
        public Task Send();
        public long ChatId { get; }
        public ChannelReader<int> Ready { get; }
    }
}
