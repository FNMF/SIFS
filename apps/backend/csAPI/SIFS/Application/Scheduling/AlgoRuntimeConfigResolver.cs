using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Application.Scheduling
{
    public class AlgoRuntimeConfigResolverService : IAlgoRuntimeConfigResolver
    {
        private readonly AlgoRuntimeConfig _defaults;
        private readonly ILogger<AlgoRuntimeConfigResolverService> _logger;

        public AlgoRuntimeConfigResolverService(
            IConfiguration configuration,
            ILogger<AlgoRuntimeConfigResolverService> logger)
        {
            _defaults = LoadDefaults(configuration.GetSection("AlgoRuntime"));
            _logger = logger;
        }

        public AlgoRuntimeConfig Resolve(AlgoModel algoModel)
        {
            var resolved = _defaults.Clone();
            if (string.IsNullOrWhiteSpace(algoModel.ReservedJson))
                return resolved;

            try
            {
                using var document = JsonDocument.Parse(algoModel.ReservedJson);
                if (!document.RootElement.TryGetProperty("runtime", out var runtime) ||
                    runtime.ValueKind != JsonValueKind.Object)
                {
                    return resolved;
                }

                ApplyRuntimeOverrides(resolved, runtime, algoModel.Name);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "算法 {AlgoName} 的 reserved_json 不是有效 JSON，使用默认运行时调度配置", algoModel.Name);
            }

            return Normalize(resolved);
        }

        private void ApplyRuntimeOverrides(AlgoRuntimeConfig target, JsonElement runtime, string algoName)
        {
            if (TryGetNonEmptyString(runtime, "resource_pool", out var resourcePool))
                target.ResourcePool = resourcePool;
            else if (runtime.TryGetProperty("resource_pool", out _))
                _logger.LogWarning("算法 {AlgoName} 的 runtime.resource_pool 无效，使用默认值 {DefaultValue}", algoName, _defaults.ResourcePool);

            if (TryGetPositiveInt(runtime, "algorithm_concurrency", out var algorithmConcurrency))
                target.AlgorithmConcurrency = algorithmConcurrency;
            else if (runtime.TryGetProperty("algorithm_concurrency", out _))
                _logger.LogWarning("算法 {AlgoName} 的 runtime.algorithm_concurrency 无效，使用默认值 {DefaultValue}", algoName, _defaults.AlgorithmConcurrency);

            if (TryGetPositiveInt(runtime, "resource_pool_concurrency", out var resourcePoolConcurrency))
                target.ResourcePoolConcurrency = resourcePoolConcurrency;
            else if (runtime.TryGetProperty("resource_pool_concurrency", out _))
                _logger.LogWarning("算法 {AlgoName} 的 runtime.resource_pool_concurrency 无效，使用默认值 {DefaultValue}", algoName, _defaults.ResourcePoolConcurrency);

            if (TryGetPositiveInt(runtime, "timeout_seconds", out var timeoutSeconds))
                target.TimeoutSeconds = timeoutSeconds;
            else if (runtime.TryGetProperty("timeout_seconds", out _))
                _logger.LogWarning("算法 {AlgoName} 的 runtime.timeout_seconds 无效，使用默认值 {DefaultValue}", algoName, _defaults.TimeoutSeconds);
        }

        private static AlgoRuntimeConfig Normalize(AlgoRuntimeConfig config)
        {
            config.ResourcePool = string.IsNullOrWhiteSpace(config.ResourcePool)
                ? "default"
                : config.ResourcePool.Trim();

            if (config.AlgorithmConcurrency <= 0)
                config.AlgorithmConcurrency = 1;

            if (config.ResourcePoolConcurrency <= 0)
                config.ResourcePoolConcurrency = 1;

            if (config.TimeoutSeconds <= 0)
                config.TimeoutSeconds = 300;

            return config;
        }

        private static AlgoRuntimeConfig LoadDefaults(IConfiguration section)
        {
            return Normalize(new AlgoRuntimeConfig
            {
                ResourcePool = section.GetValue("resource_pool", "default") ?? "default",
                AlgorithmConcurrency = section.GetValue("algorithm_concurrency", 1),
                ResourcePoolConcurrency = section.GetValue("resource_pool_concurrency", 1),
                TimeoutSeconds = section.GetValue<int?>("timeout_seconds") ?? 300
            });
        }

        private static bool TryGetNonEmptyString(JsonElement element, string propertyName, out string value)
        {
            value = string.Empty;
            if (!element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var rawValue = property.GetString();
            if (string.IsNullOrWhiteSpace(rawValue))
                return false;

            value = rawValue.Trim();
            return true;
        }

        private static bool TryGetPositiveInt(JsonElement element, string propertyName, out int value)
        {
            value = 0;
            if (!element.TryGetProperty(propertyName, out var property))
                return false;

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.TryGetInt32(out value) && value > 0,
                JsonValueKind.String => int.TryParse(property.GetString(), out value) && value > 0,
                _ => false
            };
        }
    }
}
