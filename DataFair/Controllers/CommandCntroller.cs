using Common.Services;
using Common.Services.DataBase.Interfaces;
using DataFair.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController
    {
        private readonly IOrdersGenerator ordersGenerator;
        public readonly State state;
        //public readonly OrdersManager ordersManager;
        //private readonly CancellationToken token;

        public CommandController(IOrdersGenerator ordersGenerator, State state)
        {
            this.ordersGenerator = ordersGenerator;
            this.state = state;
            //this.ordersManager = ordersManager;
            //this.token = token;
        }

        [HttpPost("PostOrder")]
        [EnableCors()]
        public string PostEmptyOrder(OrderMoq order)
        {
            if (order.Consistence)
            {
                state.ConsistanceOrders.Enqueue(order);
            }
            else
            {
                state.Orders.Enqueue(order);
            }

            return "ok";

        }

        [HttpPost("PostOrders")]
        [EnableCors()]
        public string PostEmptyOrder(List<OrderMoq> orders)
        {
            foreach (OrderMoq order in orders)
            {
                if (order.Consistence)
                {
                    state.ConsistanceOrders.Enqueue(order);
                }
                else
                {
                    state.Orders.Enqueue(order);
                }
            }
            return "ok";

        }

        [HttpPost("setcount")]
        [EnableCors()]
        public string set(CounterModel order)
        {
            state.SetCounter(order.Counter);
            return "ok";

        }

        [HttpPost("Clear")]
        [EnableCors()]
        public async Task<string> PostRequest3(CancellationToken token)
        {
            state.ClearOrders();
            return "ok";

        }
    }
}
