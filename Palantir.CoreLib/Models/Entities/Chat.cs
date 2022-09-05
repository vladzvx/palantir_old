namespace Palantir.CoreLib.Models.Entities
{
    public class Chat
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public long PairId { get; set; }
        public string? PairUsername { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public long LastMessageId { get; set; }
        public bool PairChecked { get; set; }
        public ChatInfo Info { get; set; } = new();
        public List<long> Collectors { get; set; } = new();

        public enum ChatType
        {
            Group,
            Channel
        }

        public class ChatInfo
        {
            public string? Name { get; set; }
            public string? Bio { get; set; }
            public DateTime FoundAt { get; set; } = DateTime.UtcNow;
        }
    }
}
