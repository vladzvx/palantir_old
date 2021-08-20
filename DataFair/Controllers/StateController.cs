using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DataFair.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StateController
    {
        private readonly Common.Services.StateReport state;
        public StateController(Common.Services.StateReport state)
        {
            this.state = state;
        }

        [HttpPost()]
        [EnableCors()]
        public string GetState()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(state);
        }
    }
}
