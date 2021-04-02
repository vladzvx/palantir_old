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
    public class TestController
    {
        [HttpPost()]
        [EnableCors()]
        public string CheckerAnswer()
        {
            return File.ReadAllText("settings.txt");
        }
    }
}
