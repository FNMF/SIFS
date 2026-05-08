using SIFS.Application.AlgoTaskApp;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Repositories;

namespace SIFS.Application.Scheduling
{
    public class AlgoTaskSchedulingService : IAlgoTaskSchedulingService
    {
        private readonly IAlgoTaskRepository _algoTaskRepository;
        private readonly IAlgoModelRepository _algoModelRepository;
        private readonly IAlgoRuntimeConfigResolver _runtimeConfigResolver;
        private readonly IAlgoTaskLimiterRegistryService _limiterRegistry;
        private readonly IAlgoTaskAppService _algoTaskAppService;
        private readonly ILogger<AlgoTaskSchedulingService> _logger;

        public AlgoTaskSchedulingService(
            IAlgoTaskRepository algoTaskRepository,
            IAlgoModelRepository algoModelRepository,
            IAlgoRuntimeConfigResolver runtimeConfigResolver,
            IAlgoTaskLimiterRegistryService limiterRegistry,
            IAlgoTaskAppService algoTaskAppService,
            ILogger<AlgoTaskSchedulingService> logger)
        {
            _algoTaskRepository = algoTaskRepository;
            _algoModelRepository = algoModelRepository;
            _runtimeConfigResolver = runtimeConfigResolver;
            _limiterRegistry = limiterRegistry;
            _algoTaskAppService = algoTaskAppService;
            _logger = logger;
        }

        public async Task ProcessAsync(int workerId, AlgoTaskQueueItem item, CancellationToken cancellationToken = default)
        {
            var taskId = item.TaskId;
            var algoModelId = item.AlgoModelId;

            var taskResult = await _algoTaskRepository.GetTaskByIdAsync(taskId);
            if (!taskResult.IsSuccess)
            {
                _logger.LogWarning(
                    "AlgoTaskScheduler worker {WorkerId} skipped missing task {TaskId} for algo model {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);
                return;
            }

            if (!taskResult.Data.AlgoModelId.HasValue)
            {
                await MarkPreExecutionFailureAsync(workerId, taskId, algoModelId, "algorithm model id missing");
                return;
            }

            if (taskResult.Data.AlgoModelId.Value != algoModelId)
            {
                _logger.LogWarning(
                    "AlgoTaskScheduler worker {WorkerId} found queue algo model mismatch for task {TaskId}, queued {QueuedAlgoModelId}, persisted {PersistedAlgoModelId}; persisted value will be used",
                    workerId,
                    taskId,
                    algoModelId,
                    taskResult.Data.AlgoModelId.Value);

                algoModelId = taskResult.Data.AlgoModelId.Value;
            }

            var algoModel = await _algoModelRepository.FindByIdAsync(algoModelId);
            if (algoModel == null)
            {
                await MarkPreExecutionFailureAsync(workerId, taskId, algoModelId, "algorithm model missing");
                return;
            }

            if (!algoModel.Enabled)
            {
                await MarkPreExecutionFailureAsync(workerId, taskId, algoModelId, "algorithm disabled");
                return;
            }

            if (string.IsNullOrWhiteSpace(algoModel.ApiUrl))
            {
                await MarkPreExecutionFailureAsync(workerId, taskId, algoModelId, "algorithm api url missing");
                return;
            }

            var runtimeConfig = _runtimeConfigResolver.Resolve(algoModel);
            var algorithmLimiter = _limiterRegistry.GetAlgorithmLimiter(algoModelId, runtimeConfig.AlgorithmConcurrency);
            var resourcePoolLimiter = _limiterRegistry.GetResourcePoolLimiter(runtimeConfig.ResourcePool, runtimeConfig.ResourcePoolConcurrency);

            var algorithmLimiterAcquired = false;
            var resourcePoolLimiterAcquired = false;

            try
            {
                await algorithmLimiter.WaitAsync(cancellationToken);
                algorithmLimiterAcquired = true;

                await resourcePoolLimiter.WaitAsync(cancellationToken);
                resourcePoolLimiterAcquired = true;

                await ExecuteClaimedTaskAsync(workerId, item, runtimeConfig, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "AlgoTaskScheduler worker {WorkerId} canceled while scheduling task {TaskId} for algo model {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);
            }
            finally
            {
                if (resourcePoolLimiterAcquired)
                {
                    resourcePoolLimiter.Release();
                }

                if (algorithmLimiterAcquired)
                {
                    algorithmLimiter.Release();
                }
            }
        }

        private async Task ExecuteClaimedTaskAsync(
            int workerId,
            AlgoTaskQueueItem item,
            AlgoRuntimeConfig runtimeConfig,
            CancellationToken cancellationToken)
        {
            var taskId = item.TaskId;
            var algoModelId = item.AlgoModelId;

            cancellationToken.ThrowIfCancellationRequested();

            if (!await _algoTaskRepository.TryMarkRunningAsync(taskId))
            {
                _logger.LogInformation(
                    "AlgoTaskScheduler worker {WorkerId} skipped task because it was not claimable, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}, ResourcePool: {ResourcePool}",
                    workerId,
                    taskId,
                    algoModelId,
                    runtimeConfig.ResourcePool);
                return;
            }

            try
            {
                _logger.LogInformation(
                    "AlgoTaskScheduler worker {WorkerId} claimed task {TaskId}, AlgoModelId: {AlgoModelId}, ResourcePool: {ResourcePool}",
                    workerId,
                    taskId,
                    algoModelId,
                    runtimeConfig.ResourcePool);

                var executionResult = await _algoTaskAppService.ExecuteCoreAsync(taskId);

                if (await _algoTaskRepository.TryMarkDoneAsync(taskId))
                {
                    await _algoTaskAppService.HandleExecutionSucceededAsync(taskId, executionResult);
                    _logger.LogInformation(
                        "AlgoTaskScheduler worker {WorkerId} completed task {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }
                else
                {
                    _logger.LogWarning(
                        "AlgoTaskScheduler worker {WorkerId} skipped done update because task is no longer running, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }
            }
            catch (Exception ex)
            {
                var failureReason = ToSafeFailureReason(ex);
                if (await _algoTaskRepository.TryMarkFailedAsync(taskId, failureReason))
                {
                    await _algoTaskAppService.HandleExecutionFailedAsync(taskId, failureReason);
                }
                else
                {
                    _logger.LogWarning(
                        "AlgoTaskScheduler worker {WorkerId} skipped failed update because task is no longer running, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                        workerId,
                        taskId,
                        algoModelId);
                }

                _logger.LogError(
                    ex,
                    "AlgoTaskScheduler worker {WorkerId} failed task execution, TaskId: {TaskId}, AlgoModelId: {AlgoModelId}",
                    workerId,
                    taskId,
                    algoModelId);
            }
        }

        private async Task MarkPreExecutionFailureAsync(int workerId, Guid taskId, int algoModelId, string failureReason)
        {
            if (await _algoTaskRepository.TryMarkFailedBeforeRunningAsync(taskId, failureReason))
            {
                await _algoTaskAppService.HandleExecutionFailedAsync(taskId, failureReason);
            }

            _logger.LogWarning(
                "AlgoTaskScheduler worker {WorkerId} marked task {TaskId} failed before execution, AlgoModelId: {AlgoModelId}, Reason: {FailureReason}",
                workerId,
                taskId,
                algoModelId,
                failureReason);
        }

        private static string ToSafeFailureReason(Exception ex)
        {
            return ex switch
            {
                TaskCanceledException => "algorithm timeout",
                HttpRequestException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm execution failed"
                    : ex.Message,
                InvalidOperationException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm execution failed"
                    : ex.Message,
                _ => "algorithm execution failed"
            };
        }
    }
}
