using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObserverBot.Service
{
    [ApiController]
    [Route("[controller]")]
    public class TestController
    {
        [HttpPost()]
        public async Task<string> test()
        {
            return Environment.GetEnvironmentVariable("Token")??"null";
        }
    }
}
