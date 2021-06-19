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

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController
    {
        private readonly SearchProvider searchProvider;

        public SearchController(SearchProvider searchProvider)
        {
            this.searchProvider = searchProvider;
        }

        [HttpPost("simple")]
        [EnableCors()]
        public async Task<string> simple_search(SimpleSearchRequest req,CancellationToken token)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(await searchProvider.SimpleSearch(req.Text, req.Limit, token));
        }


    }
}
