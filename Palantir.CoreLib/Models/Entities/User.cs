using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palantir.CoreLib.Models.Entities
{
    public class User
    {
        [BsonId]
        public long Id { get; set; }
        public List<string> Names { get; set; } = new List<string>();
        public List<string> Bios { get; set; } = new List<string>();
        public List<string> Usernames { get; set; } = new List<string>();

        [BsonIgnore]
        public string? Name => Names.LastOrDefault();

        [BsonIgnore]
        public string? Username => Usernames.LastOrDefault();

        [BsonIgnore]
        public string? Bio => Bios.LastOrDefault();
    }
}
