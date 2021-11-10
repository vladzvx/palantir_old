using Bot.Core.Enums;
using Bot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Models
{
{
    public class ObserverBot : IConfig
    {

        public static readonly ObserverBot Default = new ObserverBot()
        {

        };

        public ObserverBot()
        {

        }

        public string Username { get; set; }
        public string Name { get; set; }
        public bool SearchInChannels { get; set; } = true;
        public bool SearchInGroups { get; set; } = false;
        public UserStatus Status { get; set; } = UserStatus.common;
        public PrivateChatState BotState { get; set; } = PrivateChatState.Ready;
        public RequestDepth RequestDepth { get; set; } = RequestDepth.Month;
        public long Id { get; set; }
        public UserType userType { get; set; }
    }
}
