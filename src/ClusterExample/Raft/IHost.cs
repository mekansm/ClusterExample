namespace ClusterExample.Raft
{
    public interface IHost
    {
        Task InitializeAsync(string[] args) => Task.CompletedTask;
    }
}
