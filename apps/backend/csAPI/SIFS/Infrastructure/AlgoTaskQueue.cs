using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public class AlgoTaskQueue : IAlgoTaskQueue
    {
        private readonly Channel<Guid> _channel;

        public AlgoTaskQueue()
        {
            _channel = Channel.CreateUnbounded<Guid>();
        }

        public async ValueTask EnqueueAsync(Guid algoTaskId)
        {
            await _channel.Writer.WriteAsync(algoTaskId);
        }

        public ChannelReader<Guid> Reader => _channel.Reader;
    }
}
