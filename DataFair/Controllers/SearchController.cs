using Common.Models;
using Common.Services.DataBase;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using SearchRequest = Common.Models.SearchRequest;

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchProvider searchProvider;

        public SearchController(SearchProvider searchProvider)
        {
            this.searchProvider = searchProvider;
        }

        [HttpPost()]
        [EnableCors()]
        public async Task<string> SimpleSearch(SearchRequest req, CancellationToken token)
        {
            req.endDT = DateTime.UtcNow;
            req.startDT = DateTime.UtcNow.AddDays(-7);
            await await searchProvider.AsyncSearch(req.searchType,
                req.Request, req.startDT, req.endDT, req.Limit, req.isChannel, req.isGroup, token, req.ChatIds);
            return Newtonsoft.Json.JsonConvert.SerializeObject(searchProvider.searchResultReciever.ViewResults());
        }

        [HttpPost("person")]
        [EnableCors()]
        public async Task<string> GetPersonMessages(PersonSearchRequest req, CancellationToken token)
        {
            await searchProvider.PersonSearch(req.Limit, req.Id, token);
            return Newtonsoft.Json.JsonConvert.SerializeObject(searchProvider.searchResultReciever.ViewResults());
        }
    }
}
