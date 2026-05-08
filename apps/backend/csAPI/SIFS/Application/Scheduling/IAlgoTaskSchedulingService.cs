using SIFS.Infrastructure;

namespace SIFS.Application.Scheduling
{
    public interface IAlgoTaskSchedulingService
    {
        Task ProcessAsync(int workerId, AlgoTaskQueueItem item, CancellationToken cancellationToken = default);
    }
}
