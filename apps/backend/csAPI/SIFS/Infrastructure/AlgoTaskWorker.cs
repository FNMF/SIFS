using SIFS.Application.AlgoTaskApp;

namespace SIFS.Infrastructure
{
    public class AlgoTaskWorker : BackgroundService
    {
        private readonly IAlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlgoTaskWorker> _logger;

        public AlgoTaskWorker(
            IAlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AlgoTaskWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var taskId in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();

                var appService = scope.ServiceProvider
                    .GetRequiredService<IAlgoTaskAppService>();

                try
                {
                    await appService.ExecuteAsync(taskId);
                }
                catch (Exception ex)
                {
                    // 日志 / 重试
                    _logger.LogError(ex, "AlgoTaskWorker执行任务失败，TaskId: {TaskId}", taskId);
                }
            }
        }
    }
}
