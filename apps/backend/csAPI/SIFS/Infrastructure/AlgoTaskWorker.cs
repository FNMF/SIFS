using SIFS.Application.Scheduling;

namespace SIFS.Infrastructure
{
    public class AlgoTaskWorker : BackgroundService
    {
        private readonly IAlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlgoTaskWorker> _logger;
        private readonly int _workerCount;

        public AlgoTaskWorker(
            IAlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<AlgoTaskWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _workerCount = Math.Max(configuration.GetValue("AlgoTaskWorker:WorkerCount", 2), 1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AlgoTaskWorker started, WorkerCount: {WorkerCount}", _workerCount);

            var workers = Enumerable.Range(1, _workerCount)
                .Select(workerId => RunWorkerAsync(workerId, stoppingToken))
                .ToArray();

            await Task.WhenAll(workers);
        }

        private async Task RunWorkerAsync(int workerId, CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation(
                        "Dequeued algo task {TaskId} for algo model {AlgoModelId}",
                        item.TaskId,
                        item.AlgoModelId);

                    await ProcessTaskAsync(workerId, item, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AlgoTaskWorker-{WorkerId} received stop signal", workerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlgoTaskWorker-{WorkerId} consumer loop exited unexpectedly", workerId);
            }
        }

        private async Task ProcessTaskAsync(int workerId, AlgoTaskQueueItem item, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var scheduler = scope.ServiceProvider.GetRequiredService<IAlgoTaskSchedulingService>();

            try
            {
                await scheduler.ProcessAsync(workerId, item, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "AlgoTaskWorker-{WorkerId} task interrupted by stop signal, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    item.TaskId,
                    item.AlgoModelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AlgoTaskWorker-{WorkerId} failed while dispatching task to scheduler, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    item.TaskId,
                    item.AlgoModelId);
            }
        }
    }
}
