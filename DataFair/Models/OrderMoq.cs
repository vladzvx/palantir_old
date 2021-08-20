using Common;

namespace DataFair.Models
{
    public class OrderMoq
    {
        public long OrderId { get; set; }
        public long Id { get; set; }
        public string Link { get; set; }
        public long Offset { get; set; }
        public long PairId { get; set; }
        public string PairLink { get; set; }
        public long PairOffset { get; set; }
        public int Time { get; set; }
        public int RedirectCounter { get; set; }
        public OrderType Type { get; set; }


        public static implicit operator Order(OrderMoq moq)
        {
            Order order = new Order();
            order.OrderId = moq.OrderId;
            order.Id = moq.Id;
            order.Link = moq.Link ?? string.Empty;
            order.PairLink = moq.PairLink ?? string.Empty;
            order.Offset = moq.OrderId;
            order.PairId = moq.PairId;
            order.PairOffset = moq.PairOffset;
            order.Time = moq.Time;
            order.RedirectCounter = moq.RedirectCounter;
            order.Type = moq.Type;
            return order;
        }










    }
}
