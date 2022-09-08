using DotNext.Net.Cluster.Consensus.Raft;

namespace ClusterExample.Raft
{
    public interface IClusterExampleEngine
    {
        long GetValue();

        IRaftLogEntry CreateLogEntry<T>(T value);
    }
}
