using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Palantir.Common.Models;


namespace Palantir.DataHub.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        public async Task<TestModel> Test(TestModel testModel)
        {
            return testModel;
        }
    }
}
