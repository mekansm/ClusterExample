using ClusterExample.Raft;
using DotNext.Net.Cluster.Consensus.Raft;
using Microsoft.AspNetCore.Mvc;

namespace ClusterExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValueController : ControllerBase
    {
        private readonly IRaftCluster _cluster;

        public ValueController(IRaftCluster cluster)
        {
            _cluster = cluster;
        }

        [HttpGet]
        public ActionResult<long> Get()
        {
            if(_cluster.AuditTrail is IClusterExampleEngine engine)
            {
                var value = engine.GetValue();

                return value;
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> Post(long value)
        {
            if (_cluster.AuditTrail is IClusterExampleEngine engine)
            {
                var command = new UpdateCommand { Value = value };
                var log = engine.CreateLogEntry(command);

                await _cluster.ReplicateAsync(log);

                return Ok();
            }

            return NotFound();
        }
    }
}
