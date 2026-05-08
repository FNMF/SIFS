namespace SIFS.Infrastructure
{
    public class AlgoTaskQueue : AlgorithmPartitionedQueue
    {
        public AlgoTaskQueue(IConfiguration configuration, ILogger<AlgorithmPartitionedQueue> logger)
            : base(configuration, logger)
        {
        }
    }
}
