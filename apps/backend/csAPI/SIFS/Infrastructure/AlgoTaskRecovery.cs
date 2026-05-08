using Microsoft.EntityFrameworkCore;
using SIFS.Application.TaskAudits;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Repositories;

namespace SIFS.Infrastructure
{
    public class AlgoTaskRecovery : IHostedService
    {
        private const int DefaultRunningTimeoutSeconds = 1800;
        private const string FailedStrategy = "failed";
        private const string ResetPendingStrategy = "reset_pending";

        private readonly IAlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlgoTaskRecovery> _logger;

        public AlgoTaskRecovery(
            IAlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<AlgoTaskRecovery> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SIFSContext>();
            var taskAuditService = scope.ServiceProvider.GetRequiredService<ITaskAuditService>();
            var taskListRepository = scope.ServiceProvider.GetRequiredService<ITaskListRepository>();

            await RecoverPendingTasksAsync(context, taskAuditService, taskListRepository, cancellationToken);
            await RecoverTimedOutRunningTasksAsync(context, taskAuditService, taskListRepository, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task RecoverPendingTasksAsync(
            SIFSContext context,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            var pendingTasks = await context.AlgoTasks
                .AsNoTracking()
                .Where(a => a.Status == (int)AlgoTaskStatus.pending && a.DeletedAt == null)
                .Select(a => new { a.Id, a.TaskId, a.AlgoModelId })
                .ToListAsync(cancellationToken);

            foreach (var task in pendingTasks)
            {
                try
                {
                    if (!await ValidateRecoverableAlgoAsync(context, task.Id, task.TaskId, task.AlgoModelId, "pending", taskAuditService, taskListRepository, cancellationToken))
                    {
                        continue;
                    }

                    await _queue.EnqueueAsync(new AlgoTaskQueueItem(task.Id, task.AlgoModelId!.Value), cancellationToken);
                    _logger.LogInformation(
                        "Recovery requeued pending algo task {TaskId} for algo model {AlgoModelId}",
                        task.Id,
                        task.AlgoModelId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Recovery failed for pending algo task {TaskId}", task.Id);
                }
            }
        }

        private async Task RecoverTimedOutRunningTasksAsync(
            SIFSContext context,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            var timeoutSeconds = ReadPositiveInt("AlgoTaskRecovery:RunningTimeoutSeconds", DefaultRunningTimeoutSeconds);
            var strategy = NormalizeStrategy(_configuration["AlgoTaskRecovery:RunningTimeoutStrategy"]);
            var now = DateTime.UtcNow;
            var timeoutBefore = now.AddSeconds(-timeoutSeconds);

            var runningTasks = await context.AlgoTasks
                .AsNoTracking()
                .Where(a => a.Status == (int)AlgoTaskStatus.running &&
                    a.DeletedAt == null &&
                    a.StartedAt.HasValue &&
                    a.StartedAt.Value < timeoutBefore)
                .Select(a => new { a.Id, a.TaskId, a.AlgoModelId })
                .ToListAsync(cancellationToken);

            foreach (var task in runningTasks)
            {
                try
                {
                    if (strategy == ResetPendingStrategy)
                    {
                        await ResetTimedOutRunningTaskAsync(context, task.Id, task.TaskId, task.AlgoModelId, taskAuditService, taskListRepository, cancellationToken);
                    }
                    else
                    {
                        await FailTimedOutRunningTaskAsync(context, task.Id, task.TaskId, taskAuditService, taskListRepository, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Recovery failed for running algo task {TaskId}", task.Id);
                }
            }
        }

        private async Task ResetTimedOutRunningTaskAsync(
            SIFSContext context,
            Guid taskId,
            Guid parentTaskId,
            int? algoModelId,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            if (!await ValidateRecoverableAlgoAsync(context, taskId, parentTaskId, algoModelId, "processing", taskAuditService, taskListRepository, cancellationToken))
            {
                return;
            }

            var now = DateTime.UtcNow;
            var affected = await context.AlgoTasks
                .Where(a => a.Id == taskId && a.Status == (int)AlgoTaskStatus.running && a.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Status, (int)AlgoTaskStatus.pending)
                    .SetProperty(a => a.StartedAt, (DateTime?)null)
                    .SetProperty(a => a.UpdatedAt, now), cancellationToken);

            if (affected != 1)
            {
                _logger.LogInformation("Recovery skipped reset for algo task {TaskId} because state changed", taskId);
                return;
            }

            await taskAuditService.RecordTransitionAsync(
                parentTaskId,
                "processing",
                "pending",
                "task reset to pending after scheduler restart/timeout",
                null,
                new { algo_task_id = taskId, algo_model_id = algoModelId });

            await taskListRepository.RefreshProgressFromSubTasksAsync(parentTaskId);
            await _queue.EnqueueAsync(new AlgoTaskQueueItem(taskId, algoModelId!.Value), cancellationToken);
        }

        private async Task FailTimedOutRunningTaskAsync(
            SIFSContext context,
            Guid taskId,
            Guid parentTaskId,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            const string reason = "task recovered as failed after scheduler restart/timeout";
            var affected = await context.AlgoTasks
                .Where(a => a.Id == taskId && a.Status == (int)AlgoTaskStatus.running && a.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Status, (int)AlgoTaskStatus.failed)
                    .SetProperty(a => a.FailureReason, reason)
                    .SetProperty(a => a.FinishedAt, now)
                    .SetProperty(a => a.UpdatedAt, now), cancellationToken);

            if (affected != 1)
            {
                _logger.LogInformation("Recovery skipped failed update for algo task {TaskId} because state changed", taskId);
                return;
            }

            await taskAuditService.RecordTransitionAsync(
                parentTaskId,
                "processing",
                "failed",
                reason,
                null,
                new { algo_task_id = taskId });

            await taskListRepository.RefreshProgressFromSubTasksAsync(parentTaskId);
        }

        private async Task<bool> ValidateRecoverableAlgoAsync(
            SIFSContext context,
            Guid taskId,
            Guid parentTaskId,
            int? algoModelId,
            string fromStatus,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            if (!algoModelId.HasValue)
            {
                await MarkRecoverableTaskFailedAsync(context, taskId, parentTaskId, fromStatus, "algorithm model id missing during recovery", taskAuditService, taskListRepository, cancellationToken);
                return false;
            }

            var modelExists = await context.AlgoModels
                .AsNoTracking()
                .AnyAsync(x => x.Id == algoModelId.Value && x.DeletedAt == null, cancellationToken);

            if (!modelExists)
            {
                await MarkRecoverableTaskFailedAsync(context, taskId, parentTaskId, fromStatus, "algorithm model missing during recovery", taskAuditService, taskListRepository, cancellationToken);
                return false;
            }

            return true;
        }

        private async Task MarkRecoverableTaskFailedAsync(
            SIFSContext context,
            Guid taskId,
            Guid parentTaskId,
            string fromStatus,
            string reason,
            ITaskAuditService taskAuditService,
            ITaskListRepository taskListRepository,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("Recovery marked algo task {TaskId} failed: {Reason}", taskId, reason);
            var now = DateTime.UtcNow;
            var affected = await context.AlgoTasks
                .Where(a => a.Id == taskId &&
                    (a.Status == (int)AlgoTaskStatus.pending || a.Status == (int)AlgoTaskStatus.running) &&
                    a.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Status, (int)AlgoTaskStatus.failed)
                    .SetProperty(a => a.FailureReason, reason)
                    .SetProperty(a => a.FinishedAt, now)
                    .SetProperty(a => a.UpdatedAt, now), cancellationToken);

            if (affected != 1)
            {
                return;
            }

            await taskAuditService.RecordTransitionAsync(
                parentTaskId,
                fromStatus,
                "failed",
                reason,
                null,
                new { algo_task_id = taskId });

            await taskListRepository.RefreshProgressFromSubTasksAsync(parentTaskId);
        }

        private int ReadPositiveInt(string key, int defaultValue)
        {
            var value = _configuration[key];
            return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : defaultValue;
        }

        private static string NormalizeStrategy(string? strategy)
        {
            return string.Equals(strategy, ResetPendingStrategy, StringComparison.OrdinalIgnoreCase)
                ? ResetPendingStrategy
                : FailedStrategy;
        }
    }
}
