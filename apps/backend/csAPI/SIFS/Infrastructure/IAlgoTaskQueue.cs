namespace SIFS.Infrastructure
{
    public interface IAlgoTaskQueue
    {
        ValueTask EnqueueAsync(Guid algoTaskId);
    }
}
