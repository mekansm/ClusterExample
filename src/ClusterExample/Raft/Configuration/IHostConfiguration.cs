using DotNext.Net.Cluster;
using Microsoft.AspNetCore.Connections;

namespace ClusterExample.Raft.Configuration
{
    public interface IHostConfiguration
    {
        Task InitializeAsync(string[] args);

        bool IsColdStart { get; }
        Uri Url { get; }
        IEnumerable<Uri> Members { get; }
    }
}
