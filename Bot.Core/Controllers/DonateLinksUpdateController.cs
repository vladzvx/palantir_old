using Bot.Core.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpdateController
    {
        [HttpPost()]
        public async Task<string> set(LinkInfo linkInfo)
        {

            DonateLinks.keyboards.AddOrUpdate(linkInfo.Name, linkInfo, (key, val) =>
            {
                return linkInfo;
            });
            return System.Text.Json.JsonSerializer.Serialize(DonateLinks.keyboards);
        }
    }
}
