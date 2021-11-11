using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObserverBot.Service
{
    public class m
    {
        public int c { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class TestController
    {
        [HttpPost()]
        public async Task<string> test(m mm)
        {
            return Environment.GetEnvironmentVariable("Token")??"null";
        }
    }
}
