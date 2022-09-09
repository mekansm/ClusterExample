using ClusterExample.Raft;
using DotNext.Net.Cluster.Consensus.Raft.Http;

var builder = await RaftClusterApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseConsensusProtocolHandler();
app.UseSwaggerUI();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthz");
    endpoints.MapSwagger();
    endpoints.MapControllers();
});

app.Run();