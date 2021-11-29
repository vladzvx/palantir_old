using Bot.Core.Enums;
using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Models
{
    public class GateKeeperBot : IConfig
    {

        public static readonly GateKeeperBot Default = new GateKeeperBot()
        {

        };

        public GateKeeperBot()
        {

        }

        private readonly object locker = new object();

        private ChatState mode = ChatState.Common;
        public ChatState Mode { 
            get 
            {
                lock (locker)
                    return mode;
            }
            set 
            {
                lock (locker)
                    mode = value;
            } 
        }
        public string Username { get; set; }
        public string Name { get; set; }
        public bool SearchInChannels { get; set; } = true;
        public bool SearchInGroups { get; set; } = false;
        public UserStatus Status { get; set; } = UserStatus.common;
        public PrivateChatState BotState { get; set; } = PrivateChatState.Ready;
        public RequestDepth RequestDepth { get; set; } = RequestDepth.Month;
        public long Id { get; set; }
        public List<long> BannedChats { get; set; }
        public List<long> PrivilegedChats { get; set; }
        public UserType userType { get; set; }
    }
}
