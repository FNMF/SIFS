using System.Collections.Concurrent;

namespace SIFS.Application.Scheduling
{
    public class AlgoTaskLimiterRegistryService : IAlgoTaskLimiterRegistryService
    {
        private static readonly ConcurrentDictionary<int, LimiterEntry> AlgorithmLimiters = new();
        private static readonly ConcurrentDictionary<string, LimiterEntry> ResourcePoolLimiters = new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<AlgoTaskLimiterRegistryService> _logger;

        public AlgoTaskLimiterRegistryService(ILogger<AlgoTaskLimiterRegistryService> logger)
        {
            _logger = logger;
        }

        public SemaphoreSlim GetAlgorithmLimiter(int algoModelId, int concurrency)
        {
            var normalizedConcurrency = Math.Max(concurrency, 1);
            var entry = AlgorithmLimiters.GetOrAdd(
                algoModelId,
                _ => new LimiterEntry(new SemaphoreSlim(normalizedConcurrency, normalizedConcurrency), normalizedConcurrency));

            LogIfConcurrencyChanged(
                "algorithm",
                algoModelId.ToString(),
                entry.Concurrency,
                normalizedConcurrency);

            return entry.Limiter;
        }

        public SemaphoreSlim GetResourcePoolLimiter(string resourcePool, int concurrency)
        {
            var normalizedPool = string.IsNullOrWhiteSpace(resourcePool)
                ? "default"
                : resourcePool.Trim();
            var normalizedConcurrency = Math.Max(concurrency, 1);

            var entry = ResourcePoolLimiters.GetOrAdd(
                normalizedPool,
                _ => new LimiterEntry(new SemaphoreSlim(normalizedConcurrency, normalizedConcurrency), normalizedConcurrency));

            LogIfConcurrencyChanged(
                "resource pool",
                normalizedPool,
                entry.Concurrency,
                normalizedConcurrency);

            return entry.Limiter;
        }

        private void LogIfConcurrencyChanged(string limiterType, string key, int current, int requested)
        {
            if (current == requested)
            {
                return;
            }

            // SemaphoreSlim cannot be resized safely while tasks may be waiting or running.
            // Keep the first configured concurrency until application restart.
            _logger.LogWarning(
                "Runtime config requested {LimiterType} limiter {LimiterKey} concurrency {RequestedConcurrency}, but existing concurrency is {CurrentConcurrency}; restart the app to apply limiter resizing",
                limiterType,
                key,
                requested,
                current);
        }

        private sealed record LimiterEntry(SemaphoreSlim Limiter, int Concurrency);
    }
}
