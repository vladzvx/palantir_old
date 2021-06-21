using Common;
using Common.Services;
using Common.Services.DataBase.DataProcessing;
using DataFair.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController
    {
        private readonly OrdersGenerator ordersGenerator;
        public readonly State state;
        //private readonly CancellationToken token;

        public CommandController(OrdersGenerator ordersGenerator, State state)
        {
            this.ordersGenerator = ordersGenerator;
            this.state = state;
            //this.token = token;
        }

        [HttpPost("GetFullChannel")]
        [EnableCors()]
        public string PostRequest()
        {
            ordersGenerator.CreateGetFullChannelOrders(800).Wait();
            return "ok";
            
        }

        [HttpPost("PostEmptyOrder")]
        [EnableCors()]
        public string PostEmptyOrder()
        {
            state.Orders.Enqueue(new Order() { Type = OrderType.Empty });
            return "ok";

        }

        [HttpPost("PostOrder")]
        [EnableCors()]
        public string PostEmptyOrder(OrderMoq order)
        {
            state.MaxPriorityOrders.Enqueue(order);
            return "ok";

        }

        [HttpPost("GetGroupsHistory")]
        [EnableCors()]
        public string PostRequest2()
        {
            ordersGenerator.CreateGroupHistoryLoadingOrders().Wait();
            return "ok";

        }

        [HttpPost("GetHistory")]
        [EnableCors()]
        public string PostRequest3()
        {
            ordersGenerator.CreateHistoryLoadingOrders().Wait();
            return "ok";

        }
        [HttpPost("GetUpdates")]
        [EnableCors()]
        public async Task<string> PostRequest4(CancellationToken token)
        {
            await ordersGenerator.CreateUpdateOrders(token);
            return "ok";
        }
    }
}
