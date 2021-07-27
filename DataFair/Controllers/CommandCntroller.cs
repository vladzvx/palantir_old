using Common;
using Common.Services;
using Common.Services.DataBase;
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

        [HttpPost("PostOrder")]
        [EnableCors()]
        public string PostEmptyOrder(OrderMoq order)
        {
            state.MaxPriorityOrders.Enqueue(order);
            return "ok";

        }


        [HttpPost("GetHistory")]
        [EnableCors()]
        public async Task<string> PostRequest3(CancellationToken token)
        {
            state.ClearOrders();
            await ordersGenerator.SetOrderUnGeneratedStatus(token);
            await ordersGenerator.GetHistoryOrders(token);
            return "ok";

        }

        [HttpPost("GetUpdates")]
        [EnableCors()]
        public async Task<string> PostRequest8(CancellationToken token)
        {
            state.ClearOrders();
            await ordersGenerator.SetOrderUnGeneratedStatus(token);
            await ordersGenerator.GetUpdatesOrders(token);
            return "ok";

        }


        [HttpPost("GetNewGroups")]
        [EnableCors()]
        public async Task<string> PostRequest5(CancellationToken token)
        {
            state.ClearOrders();
            await ordersGenerator.SetOrderUnGeneratedStatus(token);
            await ordersGenerator.GetHistoryOrders(token);
            return "ok";
        }

        [HttpPost("create_orders")]
        [EnableCors()]
        public async Task<string> PostRequest6(CancellationToken token)
        {
            state.ClearOrders();
            await ordersGenerator.SetOrderUnGeneratedStatus(token);
            await ordersGenerator.GetNewGroupsOrders(token);
            await ordersGenerator.GetHistoryOrders(token);
            return "ok";
        }

        [HttpPost("restore")]
        [EnableCors()]
        public async Task<string> PostRequest7(CancellationToken token)
        {
            state.ClearOrders();
            await ordersGenerator.SetOrderUnGeneratedStatus(token);
            await ordersGenerator.RestoreLostGroups(token);
            await ordersGenerator.RestoreLostGroupsSetStatus(token);
            return "ok";
        }
    }
}
