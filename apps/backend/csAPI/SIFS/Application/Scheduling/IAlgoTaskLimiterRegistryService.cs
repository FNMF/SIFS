using System.Threading;

namespace SIFS.Application.Scheduling
{
    public interface IAlgoTaskLimiterRegistryService
    {
        SemaphoreSlim GetAlgorithmLimiter(int algoModelId, int concurrency);
        SemaphoreSlim GetResourcePoolLimiter(string resourcePool, int concurrency);
    }
}
