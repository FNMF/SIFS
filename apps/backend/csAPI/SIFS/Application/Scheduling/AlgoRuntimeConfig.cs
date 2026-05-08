using System.Text.Json.Serialization;

namespace SIFS.Application.Scheduling
{
    public class AlgoRuntimeConfig
    {
        [JsonPropertyName("resource_pool")]
        public string ResourcePool { get; set; } = "default";

        [JsonPropertyName("algorithm_concurrency")]
        public int AlgorithmConcurrency { get; set; } = 1;

        [JsonPropertyName("resource_pool_concurrency")]
        public int ResourcePoolConcurrency { get; set; } = 1;

        [JsonPropertyName("timeout_seconds")]
        public int? TimeoutSeconds { get; set; } = 300;

        public AlgoRuntimeConfig Clone()
        {
            return new AlgoRuntimeConfig
            {
                ResourcePool = ResourcePool,
                AlgorithmConcurrency = AlgorithmConcurrency,
                ResourcePoolConcurrency = ResourcePoolConcurrency,
                TimeoutSeconds = TimeoutSeconds
            };
        }
    }
}
