using Common;
using Common.Models;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.DataProcessing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        public CommandController(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
        }

        [HttpPost("kill")]
        [EnableCors()]
        public string kill()
        {
            cancellationTokenSource.Cancel();
            return "ok";
        }
    }
}
