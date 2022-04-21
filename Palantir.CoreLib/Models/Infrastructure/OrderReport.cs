using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palantir.CoreLib.Models.Infrastructure
{
    public class OrderReport
    {
        public OrderStatus Status { get; set; } 
        public Guid OrderId { get; set; }
        public string? Note { get; set; }
        public Exception? Exception { get; set; }

        public OrderReport(Order order, OrderStatus status, string? note)
        {
            OrderId = order.Id;
            Status = status;
            Note = note;
        }

        public enum OrderStatus
        {
            Success,
            Fail
        }
    }
}
