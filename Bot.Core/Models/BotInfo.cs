using Common.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Models
{
    public class UserInfo
    {
        public ObjectId FirstPageId { get; set; }
        public double Score { get; set; }
        public int TotalMessages { get; set; }
        public int BadMessages { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Normal;
    }
    public class BotInfo
    {
        [BsonId]
        public long _id { get; set; }

        public string Username { get; set; }
    }
}
