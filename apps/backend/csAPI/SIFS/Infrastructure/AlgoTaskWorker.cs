using SIFS.Application.AlgoTaskApp;
using SIFS.Infrastructure.Repositories;

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
            var taskId = item.TaskId;
            var algoModelId = item.AlgoModelId;

            using var scope = _scopeFactory.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IAlgoTaskAppService>();
            var algoTaskRepository = scope.ServiceProvider.GetRequiredService<IAlgoTaskRepository>();

            try
            {
                stoppingToken.ThrowIfCancellationRequested();

                if (!await algoTaskRepository.TryMarkRunningAsync(taskId))
                {
                    _logger.LogInformation(
                        "AlgoTaskWorker-{WorkerId} skipped task because it was not claimable, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                    return;
                }

                _logger.LogInformation(
                    "AlgoTaskWorker-{WorkerId} claimed task, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);

                var executionResult = await appService.ExecuteCoreAsync(taskId);

                if (await algoTaskRepository.TryMarkDoneAsync(taskId))
                {
                    await appService.HandleExecutionSucceededAsync(taskId, executionResult);
                    _logger.LogInformation(
                        "AlgoTaskWorker-{WorkerId} completed task, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }
                else
                {
                    _logger.LogWarning(
                        "AlgoTaskWorker-{WorkerId} skipped done update because task is no longer running, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "AlgoTaskWorker-{WorkerId} task interrupted by stop signal, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);
            }
            catch (Exception ex)
            {
                var failureReason = ToSafeFailureReason(ex);

                try
                {
                    if (await algoTaskRepository.TryMarkFailedAsync(taskId, failureReason))
                    {
                        await appService.HandleExecutionFailedAsync(taskId, failureReason);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "AlgoTaskWorker-{WorkerId} skipped failed update because task is no longer running, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                            workerId,
                            taskId,
                            algoModelId);
                    }
                }
                catch (Exception failureHandlingError)
                {
                    _logger.LogError(
                        failureHandlingError,
                        "AlgoTaskWorker-{WorkerId} failed while handling task failure, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }

                _logger.LogError(
                    ex,
                    "AlgoTaskWorker-{WorkerId} failed task execution, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);
            }
        }

        private static string ToSafeFailureReason(Exception ex)
        {
            return ex switch
            {
                TaskCanceledException => "algorithm request timeout",
                HttpRequestException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm request failed"
                    : ex.Message,
                InvalidOperationException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm invocation failed"
                    : ex.Message,
                _ => "algorithm invocation failed"
            };
        }
    }
}
