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
            _logger.LogInformation("AlgoTaskWorker启动，WorkerCount: {WorkerCount}", _workerCount);

            var workers = Enumerable.Range(1, _workerCount)
                .Select(workerId => RunWorkerAsync(workerId, stoppingToken))
                .ToArray();

            await Task.WhenAll(workers);
        }

        private async Task RunWorkerAsync(int workerId, CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var taskId in _queue.Reader.ReadAllAsync(stoppingToken))
                {
                    await ProcessTaskAsync(workerId, taskId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AlgoTaskWorker-{WorkerId} 收到停止信号", workerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlgoTaskWorker-{WorkerId} 消费循环异常退出", workerId);
            }
        }

        private async Task ProcessTaskAsync(int workerId, Guid taskId, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IAlgoTaskAppService>();
            var algoTaskRepository = scope.ServiceProvider.GetRequiredService<IAlgoTaskRepository>();

            try
            {
                stoppingToken.ThrowIfCancellationRequested();

                if (!await algoTaskRepository.TryMarkRunningAsync(taskId))
                {
                    _logger.LogInformation("AlgoTaskWorker-{WorkerId} 未抢占任务，跳过执行，TaskId: {TaskId}", workerId, taskId);
                    return;
                }

                _logger.LogInformation("AlgoTaskWorker-{WorkerId} 已抢占任务，开始执行，TaskId: {TaskId}", workerId, taskId);

                var executionResult = await appService.ExecuteCoreAsync(taskId);

                if (await algoTaskRepository.TryMarkDoneAsync(taskId))
                {
                    await appService.HandleExecutionSucceededAsync(taskId, executionResult);
                    _logger.LogInformation("AlgoTaskWorker-{WorkerId} 任务执行成功，TaskId: {TaskId}", workerId, taskId);
                }
                else
                {
                    _logger.LogWarning("AlgoTaskWorker-{WorkerId} 任务已不处于 running，跳过完成状态覆盖，TaskId: {TaskId}", workerId, taskId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AlgoTaskWorker-{WorkerId} 任务因停止信号中断，TaskId: {TaskId}", workerId, taskId);
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
                        _logger.LogWarning("AlgoTaskWorker-{WorkerId} 任务已不处于 running，跳过失败状态覆盖，TaskId: {TaskId}", workerId, taskId);
                    }
                }
                catch (Exception failureHandlingError)
                {
                    _logger.LogError(failureHandlingError, "AlgoTaskWorker-{WorkerId} 处理任务失败状态时出错，TaskId: {TaskId}", workerId, taskId);
                }

                _logger.LogError(ex, "AlgoTaskWorker-{WorkerId} 执行任务失败，TaskId: {TaskId}", workerId, taskId);
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
