using DotNext.Net.Cluster;
using Microsoft.AspNetCore.Connections;
using System.Net;
using System.Net.Sockets;

namespace ClusterExample.Raft.Configuration
{
    public class KubernetesConfiguration : IHostConfiguration
    {
        public bool IsColdStart => false;

        public Uri Url { get; private set; }

        public IEnumerable<Uri> Members { get; private set; }

        public async Task InitializeAsync(string[] args)
        {
            Url = GetUrl();
            Members = await GetMembers();
        }

        private Uri GetUrl()
        {
            var url = Environment.GetEnvironmentVariable("CLUSTER_IP");

            if (string.IsNullOrWhiteSpace(url)) throw new Exception("CLUSTER_IP environment variable not set.");

            if (!Uri.TryCreate($"http://{url}", UriKind.Absolute, out var result)) throw new Exception($"'{url}' is not a valid URL.");

            return result;
        }

        private async Task<IEnumerable<Uri>> GetMembers()
        {
            var nodes = Environment.GetEnvironmentVariable("CLUSTER_NODE_COUNT");

            if (string.IsNullOrWhiteSpace(nodes)) throw new Exception("CLUSTER_NODE_COUNT environment variable not set.");

            if(!int.TryParse(nodes, out var nodeCount)) throw new Exception("CLUSTER_NODE_COUNT environment variable not set to a number.");

            for (var i = 0; i < 60; i++)
            {
                try
                {
                    var entry = Dns.GetHostEntry("cluster-example-discovery", AddressFamily.InterNetwork);

                    if (entry.AddressList.Length == nodeCount)
                    {
                        var result = new List<Uri>();

                        foreach (var address in entry.AddressList)
                        {
                            var builder = new UriBuilder(Url)
                            {
                                Host = address.ToString()
                            };

                            result.Add(builder.Uri);
                        }

                        return result;
                    }
                }
                catch
                {
                    // DNS might not be ready
                }

                await Task.Delay(1000);
            }

            throw new Exception("Unable to resolve cluster members before timeouut.");
        }
    }
}
