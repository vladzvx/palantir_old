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
            return File.Exists("/root/DataFair/settings.txt").ToString();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class PathController
    {
        [HttpPost()]
        [EnableCors()]
        public string CheckerAnswer()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }


    [ApiController]
    [Route("[controller]")]
    public class SettingsTestController
    {
        [HttpPost()]
        [EnableCors()]
        public string CheckerAnswer()
        {
            return File.ReadAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Constants.SettingsFilename));
        }
    }
}
