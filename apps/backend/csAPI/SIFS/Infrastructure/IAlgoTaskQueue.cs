using System.Threading.Channels;

namespace SIFS.Infrastructure
{
    public interface IAlgoTaskQueue
    {
        ValueTask EnqueueAsync(Guid algoTaskId);
        ChannelReader<Guid> Reader { get; }
    }
}
