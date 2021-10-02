using Bot.Core.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface IConfig
    {
        [BsonId]
        public long Id { get; set; }
        public UserStatus Status { get; set; }
        public PrivateChatState BotState { get; set; }


    }
}
