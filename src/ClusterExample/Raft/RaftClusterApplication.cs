using ClusterExample.Raft.Configuration;
using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Consensus.Raft.Http;
using DotNext.Net.Cluster.Messaging;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

namespace ClusterExample.Raft
{
    public static class RaftClusterApplication
    {

        public static async Task<WebApplicationBuilder> CreateBuilder(string[] args)
        {
            var host = GetHostConfiguration();

            await host.InitializeAsync(args);

            var nodes = host.Members.ToList();
            var configuration = new Dictionary<string, string>
            {
                {"partitioning", "false"},
                {"lowerElectionTimeout", "150" },
                {"upperElectionTimeout", "300" },
                {"requestTimeout", "00:10:00"},
                {"publicEndPoint", host.Url.ToString()},
                {"coldStart", host.IsColdStart.ToString()},
                {"requestJournal:memoryLimit", "5" },
                {"requestJournal:expiration", "00:01:00" },
                {ClusterExampleEngine.LogLocation, $"state/{Guid.NewGuid()}"}
            };

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddInMemoryCollection(configuration);

            builder.Services.AddSingleton<IInputChannel, RedirectMesssageHandler>();
            builder.Services.AddTransient<IClusterExampleProxy, ClusterExampleProxy>();

            builder.Services.UseInMemoryConfigurationStorage(AddClusterMembers);
            builder.Services.UsePersistenceEngine<IClusterExampleEngine, ClusterExampleEngine>();

            builder.JoinCluster();

            return builder;

            void AddClusterMembers(IDictionary<ClusterMemberId, UriEndPoint> members)
            {
                foreach (var node in nodes)
                {
                    var address = new UriEndPoint(node);

                    members.Add(ClusterMemberId.FromEndPoint(address), address);
                }
            }
        }

        private static IHostConfiguration GetHostConfiguration()
        {
            if (Debugger.IsAttached) return new VisualStudioConfiguration();
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLUSTER_IP"))) return new KubernetesConfiguration();

            throw new Exception("Unable to determine host configuration");
        }
    }
}
