using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StateController
    {
        private readonly State state;
        public StateController(State state)
        {
            this.state = state;
        }

        [HttpPost()]
        [EnableCors()]
        public string GetState()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(state.GetStateReport());
        }
    }
}
