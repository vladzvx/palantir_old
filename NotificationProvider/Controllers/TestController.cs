using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationProvider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController
    {
        public IRabbitMQSettings settings;
        public TestController(IRabbitMQSettings settings)
        {
            this.settings = settings;
        }
        [HttpPost()]
        public async Task<string> test(CancellationToken token)
        {
            return System.Text.Json.JsonSerializer.Serialize(settings);
        }
    }
}
