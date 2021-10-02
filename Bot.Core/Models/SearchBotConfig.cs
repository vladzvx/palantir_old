using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Services;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Models
{
    public class SearchBot : IConfig
    {
         
        public static readonly SearchBot Default = new SearchBot()
        {
            SearchInChannels = true,
            SearchInGroups = false,
            RequestDepth = RequestDepth.Inf
        };

        public SearchBot()
        {

        }

        public bool SearchInChannels { get; set; } = true;
        public bool SearchInGroups { get; set; } = false;
        public UserStatus Status { get; set; } = UserStatus.common;
        public PrivateChatState BotState { get; set; } = PrivateChatState.Ready;
        public RequestDepth RequestDepth { get; set; } = RequestDepth.Month;
        public long Id { get; set; }
    }
}
