using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class OrderProto
    {
        public Order ToOrder()
        {
            return new Order()
            {
                Type = OrderType.Empty   
            };
        }
    }
}
