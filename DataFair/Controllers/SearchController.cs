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

        [HttpPost()]
        [EnableCors()]
        public async Task<string> SimpleSearch(SearchRequest req,CancellationToken token)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(await searchProvider.CommonSearch(req.searchType,
                req.Request,req.startDT,req.endDT,req.Limit,req.isChannel, req.isGroup,token,req.ChatIds));
        }

        [HttpPost("person")]
        [EnableCors()]
        public async Task<string> GetPersonMessages(PersonSearchRequest req, CancellationToken token)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(await searchProvider.PersonSearch(req.Limit,req.Id,token));
        }
    }
}
