using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palantir.CoreLib.Models.Infrastructure
{
    public class Order
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public long ChatId { get; set; }
        public long Offset { get; set; }
        public OrderType Type { get; set; }
        public Order? LinkedEntity { get; set; }

        public enum OrderType
        {
            Target,
            Pair,
            GetFullChannel,
            Info
        }
    }
}
