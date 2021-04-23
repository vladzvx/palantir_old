using Common;
using Common.Models;
using DataFair.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CommandController
    {
        private readonly State state;
        private readonly OrdersCreator creator;
        public CommandController(State state)
        {
            this.state = state;
            //creator = new  OrdersCreator(state);
        }

        [HttpPost()]
        [EnableCors()]
        public string PostRequest(OrderCreationRequest request)
        {
            if (request != null &&
                request.Type != null &&
                request.Number != 0 &&
                Enum.TryParse(request.Type, out OrderType orderType))
            {
                creator.CreateGetFullChannelOrders(request.Number).Wait();
                return "Ok"!;
            }
            else return "fail";
            
        }
    }
}
