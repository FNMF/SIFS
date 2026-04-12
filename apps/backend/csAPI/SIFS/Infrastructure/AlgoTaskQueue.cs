using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public class AlgoTaskQueue : IAlgoTaskQueue
    {
        private readonly Channel<Guid> _channel;

        public AlgoTaskQueue()
        {
            _channel = Channel.CreateBounded<Guid>(
                new BoundedChannelOptions(1000)     // 1000容量防止OOM
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public async ValueTask EnqueueAsync(Guid algoTaskId)
        {
            await _channel.Writer.WriteAsync(algoTaskId);
        }

        public ChannelReader<Guid> Reader => _channel.Reader;
    }
}
