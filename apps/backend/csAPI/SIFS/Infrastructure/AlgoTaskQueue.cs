using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public class AlgoTaskQueue : IAlgoTaskQueue
    {
        private readonly Channel<Guid> _channel;

        public AlgoTaskQueue(IConfiguration configuration)
        {
            var capacity = Math.Max(configuration.GetValue("AlgoTaskWorker:QueueCapacity", 1000), 1);
            _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(Guid algoTaskId)
        {
            await _channel.Writer.WriteAsync(algoTaskId);
        }

        public ChannelReader<Guid> Reader => _channel.Reader;
    }
}
