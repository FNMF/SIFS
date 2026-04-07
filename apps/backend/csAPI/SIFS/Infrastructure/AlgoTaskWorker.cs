using SIFS.Application.DetectionTaskApp;

namespace SIFS.Infrastructure
{
    public class AlgoTaskWorker : BackgroundService
    {
        private readonly AlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public AlgoTaskWorker(
            AlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var taskId in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();

                var appService = scope.ServiceProvider
                    .GetRequiredService<IDetectionTaskAppService>();

                try
                {
                    // await appService.ExecuteAsync(taskId);
                }
                catch (Exception ex)
                {
                    // 这里可以加日志 / 重试
                }
            }
        }
    }
}
