using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public class AlgoTaskQueue : IAlgoTaskQueue
    {
        private readonly Channel<AlgoTaskQueueItem> _channel;
        private readonly ILogger<AlgoTaskQueue> _logger;

        public AlgoTaskQueue(IConfiguration configuration, ILogger<AlgoTaskQueue> logger)
        {
            _logger = logger;
            var capacity = Math.Max(configuration.GetValue("AlgoTaskWorker:QueueCapacity", 1000), 1);
            _channel = Channel.CreateBounded<AlgoTaskQueueItem>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(AlgoTaskQueueItem item, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(item, cancellationToken);
            _logger.LogInformation("Enqueued algo task {TaskId} for algo model {AlgoModelId}", item.TaskId, item.AlgoModelId);
        }

        public ChannelReader<AlgoTaskQueueItem> Reader => _channel.Reader;
    }
}
