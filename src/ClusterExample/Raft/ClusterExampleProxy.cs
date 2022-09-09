using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Messaging;

namespace ClusterExample.Raft
{
    public class ClusterExampleProxy : IClusterExampleProxy
    {
        private readonly IRaftCluster _cluster;

        public ClusterExampleProxy(IRaftCluster cluster)
        {
            _cluster = cluster;
        }

        public long GetValue()
        {
            if (_cluster.AuditTrail is not IClusterExampleEngine engine) throw new Exception("AuditTrail is not of type IClusterExampleEngine");

            var value = engine.GetValue();

            return value;
        }

        public async Task UpdateValue(long value)
        {
            if (_cluster is not IMessageBus bus) throw new Exception("IRaftCluster is not of type IMessageBus");
          
            var command = new UpdateCommand { Value = value };
            var message = new JsonMessage<UpdateCommand>(RedirectMesssageHandler.MessaageName, command);

            await bus.LeaderRouter.SendSignalAsync(message);           
        }
    }
}
