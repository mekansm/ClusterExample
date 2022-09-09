using ClusterExample.Raft;
using Microsoft.AspNetCore.Mvc;

namespace ClusterExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValueController : ControllerBase
    {
        private readonly IClusterExampleProxy _proxy;

        public ValueController(IClusterExampleProxy proxy)
        {
            _proxy = proxy;
        }

        [HttpGet]
        public ActionResult<long> Get()
        {
            return _proxy.GetValue();
        }

        [HttpPost]
        public async Task<ActionResult> Post(long value)
        {
            await _proxy.UpdateValue(value);
            
            return Ok();
        }
    }
}
