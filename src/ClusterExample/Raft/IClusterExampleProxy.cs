namespace ClusterExample.Raft
{
    public interface IClusterExampleProxy
    {
        long GetValue();

        Task UpdateValue(long value);
    }
}
