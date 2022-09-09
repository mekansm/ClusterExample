using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Messaging;

namespace ClusterExample.Raft
{
    public class RedirectMesssageHandler : IInputChannel
    {
        public const string MessaageName = "RedirectToLeader";
        
        private readonly IServiceProvider _services;

        public RedirectMesssageHandler(IServiceProvider services)
        {
            _services = services;
        }

        public async  Task<IMessage> ReceiveMessage(ISubscriber sender, IMessage message, object? context, CancellationToken token)
        {
            await HandleMessage(message);

            return new TextMessage("Ok", MessaageName + "Response");
        }

        public async Task ReceiveSignal(ISubscriber sender, IMessage signal, object? context, CancellationToken token)
        {
            await HandleMessage(signal);
        }

        private async Task HandleMessage(IMessage signal)
        {
            if (signal.Name == MessaageName)
            {
                var message = await JsonMessage<UpdateCommand>.FromJsonAsync(signal);
                var cluster = _services.GetRequiredService<IRaftCluster>();

                if(cluster.AuditTrail is IClusterExampleEngine engine)
                {
                    var log = engine.CreateLogEntry(message);

                    await cluster.ReplicateAsync(log);
                }
            }
        }
    }
}
