using DotNext.Threading;

namespace ClusterExample.Raft
{

    public partial class ClusterExampleEngine
    {
        private sealed class ClusterExampleSnapshotBuilder : IncrementalSnapshotBuilder
        {
            private readonly ILogger<ClusterExampleSnapshotBuilder> _logger;
            private long _value;

            public ClusterExampleSnapshotBuilder(in SnapshotBuilderContext context, ILoggerFactory loggerFactory)
                : base(context)
            {
                _logger = loggerFactory.CreateLogger<ClusterExampleSnapshotBuilder>();
            }

            protected override async ValueTask ApplyAsync(LogEntry entry)
            {
                var command = await entry.DeserializeFromJsonAsync();

                switch (command)
                {
                    case UpdateCommand update:
                        ApplyUpdateCommand(update);
                        break;
                    default:
                        _logger.LogWarning($"Unknown type encountered while deserializing: '{command?.GetType()}'");
                        break;
                }
            }

            public override ValueTask WriteToAsync<TWriter>(TWriter writer, CancellationToken token)
                => writer.WriteAsync(_value, token);
            private void ApplyUpdateCommand(UpdateCommand update)
            {
                _value.VolatileWrite(update.Value);
                _logger.LogInformation($"Setting value to {update.Value}");
            }
        }
    }
}
