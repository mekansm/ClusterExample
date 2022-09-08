using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Threading;

namespace ClusterExample.Raft
{

    public partial class ClusterExampleEngine : MemoryBasedStateMachine, IClusterExampleEngine
    {
        internal const string LogLocation = "logLocation";

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ClusterExampleEngine> _logger;

        private long _content;

        public ClusterExampleEngine(IConfiguration configuration, ILoggerFactory loggerFactory)
            : base(configuration.GetValue<string>(LogLocation), 50)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ClusterExampleEngine>();
        }

        public IRaftLogEntry CreateLogEntry<T>(T value) => CreateJsonLogEntry(value);
        public long GetValue() => _content.VolatileRead();

        protected override async ValueTask ApplyAsync(LogEntry entry)
        {
            if (entry.Length > 0)
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
        }

        protected override SnapshotBuilder CreateSnapshotBuilder(in SnapshotBuilderContext context)
        {
            return new ClusterExampleSnapshotBuilder(context, _loggerFactory);
        }

        private void ApplyUpdateCommand(UpdateCommand update)
        {
            _content.VolatileWrite(update.Value);
            _logger.LogInformation($"Accepting value {update.Value}");
        }
    }
}
