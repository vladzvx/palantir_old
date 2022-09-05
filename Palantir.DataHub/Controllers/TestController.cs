using Microsoft.AspNetCore.Mvc;


namespace Palantir.DataHub.Controllers
{
    public class TestModel
    {
        public string? Text { get; set; }
    }

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
