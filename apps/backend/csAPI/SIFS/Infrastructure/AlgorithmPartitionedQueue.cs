using System.Collections.Concurrent;
using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public class AlgorithmPartitionedQueue : IAlgoTaskQueue
    {
        private const int DefaultPartitionCapacity = 100;

        private readonly ConcurrentDictionary<int, Channel<AlgoTaskQueueItem>> _partitions = new();
        private readonly ConcurrentDictionary<int, byte> _partitionPumps = new();
        private readonly Channel<AlgoTaskQueueItem> _readerChannel;
        private readonly ILogger<AlgorithmPartitionedQueue> _logger;
        private readonly int _partitionCapacity;

        public AlgorithmPartitionedQueue(IConfiguration configuration, ILogger<AlgorithmPartitionedQueue> logger)
        {
            _logger = logger;
            _partitionCapacity = ReadPositiveInt(
                configuration,
                "AlgoTaskQueue:PartitionCapacity",
                DefaultPartitionCapacity);

            var readerCapacity = ReadPositiveInt(configuration, "AlgoTaskWorker:QueueCapacity", 1000);
            _readerChannel = Channel.CreateBounded<AlgoTaskQueueItem>(new BoundedChannelOptions(readerCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(AlgoTaskQueueItem item, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.TaskId == Guid.Empty)
            {
                throw new ArgumentException("TaskId must not be empty.", nameof(item));
            }

            if (item.AlgoModelId <= 0)
            {
                throw new ArgumentException("AlgoModelId must be a positive integer.", nameof(item));
            }

            var partition = GetOrCreatePartition(item.AlgoModelId);
            await partition.Writer.WriteAsync(item, cancellationToken);

            _logger.LogInformation(
                "Enqueued algo task {TaskId} for algo model {AlgoModelId}, partition capacity {PartitionCapacity}",
                item.TaskId,
                item.AlgoModelId,
                _partitionCapacity);
        }

        public ChannelReader<AlgoTaskQueueItem> Reader => _readerChannel.Reader;

        public IReadOnlyCollection<int> GetPartitionIds()
        {
            return _partitions.Keys.ToArray();
        }

        public int PartitionCapacity => _partitionCapacity;

        private Channel<AlgoTaskQueueItem> GetOrCreatePartition(int algoModelId)
        {
            if (algoModelId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(algoModelId), "AlgoModelId must be a positive integer.");
            }

            var partition = _partitions.GetOrAdd(algoModelId, CreatePartition);
            EnsurePartitionPump(algoModelId, partition);
            return partition;
        }

        private Channel<AlgoTaskQueueItem> CreatePartition(int algoModelId)
        {
            _logger.LogInformation(
                "Created algo task queue partition for algo model {AlgoModelId}, partition capacity {PartitionCapacity}",
                algoModelId,
                _partitionCapacity);

            return Channel.CreateBounded<AlgoTaskQueueItem>(new BoundedChannelOptions(_partitionCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        }

        private void EnsurePartitionPump(int algoModelId, Channel<AlgoTaskQueueItem> partition)
        {
            if (!_partitionPumps.TryAdd(algoModelId, 0))
            {
                return;
            }

            _ = Task.Run(() => PumpPartitionAsync(algoModelId, partition));
        }

        private async Task PumpPartitionAsync(int algoModelId, Channel<AlgoTaskQueueItem> partition)
        {
            try
            {
                await foreach (var item in partition.Reader.ReadAllAsync())
                {
                    await _readerChannel.Writer.WriteAsync(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Algo task queue partition pump failed for algo model {AlgoModelId}", algoModelId);
                _partitionPumps.TryRemove(algoModelId, out _);
            }
        }

        private static int ReadPositiveInt(IConfiguration configuration, string key, int defaultValue)
        {
            var value = configuration[key];
            return int.TryParse(value, out var parsed) && parsed > 0
                ? parsed
                : defaultValue;
        }
    }
}
