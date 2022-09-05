namespace Palantir.CoreLib.Models.Infrastructure
{
    public class Order
    {
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
