using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Models
{
    public class BotInfo
    {
        [BsonId]
        public long _id { get; set; }

        public string Username { get; set; }
    }
}
