using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public interface IAlgoTaskQueue
    {
        ValueTask EnqueueAsync(AlgoTaskQueueItem item, CancellationToken cancellationToken = default);
        ChannelReader<AlgoTaskQueueItem> Reader { get; }
    }
}
