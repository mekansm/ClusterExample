using ClusterExample.Raft;
using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Consensus.Raft.Http;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

var scheme = "http";
var hostname = GetHostName();
var port = GetPort();

var configuration = new Dictionary<string, string>
            {
                {"partitioning", "false"},
                {"lowerElectionTimeout", "150" },
                {"upperElectionTimeout", "300" },
                {"requestTimeout", "00:10:00"},
                {"publicEndPoint", $"{scheme}://{hostname}:{port}"},
                {"coldStart", Debugger.IsAttached.ToString()},
                {"requestJournal:memoryLimit", "5" },
                {"requestJournal:expiration", "00:01:00" },
                {ClusterExampleEngine.LogLocation, $"state/{Guid.NewGuid()}"}
            };

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(configuration);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.UseInMemoryConfigurationStorage(AddClusterMembers);
builder.Services.UsePersistenceEngine<IClusterExampleEngine, ClusterExampleEngine>();

builder.JoinCluster();

var app = builder.Build();

app.UseConsensusProtocolHandler();
app.UseSwaggerUI();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSwagger();
    endpoints.MapControllers();
});

app.Run();


string GetHostName()
{
    if (Debugger.IsAttached)
    {
        return "localhost";
    }

    return Environment.GetEnvironmentVariable("CLUSTER_IP") ?? throw new Exception("CLUSTER_IP environment variable not set.");
}

int GetPort()
{
    var port = 80;

    if (Debugger.IsAttached)
    {
        var possiblePort = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(";")
                                                                         .Select(x => new Uri(x))
                                                                         .Where(x => x.Scheme == scheme)
                                                                         .Select(x => x.Port)
                                                                         .FirstOrDefault();

        if (possiblePort.HasValue)
        {
            return possiblePort.Value;
        }
    }

    return port;
}

void AddClusterMembers(IDictionary<ClusterMemberId, UriEndPoint> members)
{
    if (Debugger.IsAttached)
    {
        var url = new Uri($"{scheme}://{hostname}:{port}", UriKind.Absolute);
        var address = new UriEndPoint(url);

        members.Add(ClusterMemberId.FromEndPoint(address), address);
    }
    else
    {
        // dns service discovery
    }
}

