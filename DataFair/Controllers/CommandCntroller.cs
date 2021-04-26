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
        private readonly OrdersManager creator;
        public CommandController(State state)
        {
            this.state = state;
            //creator = new  OrdersCreator(state);
        }

        [HttpPost()]
        [EnableCors()]
        public string PostRequest()
        {
            OrdersManager.EnableGetFullChannelOrdersGen();
            return "ok";
            
        }
    }
}
