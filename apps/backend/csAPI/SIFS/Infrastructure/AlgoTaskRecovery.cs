using Microsoft.EntityFrameworkCore;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;

namespace SIFS.Infrastructure
{
    public class AlgoTaskRecovery : IHostedService
    {
        private readonly IAlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlgoTaskRecovery> _logger;

        public AlgoTaskRecovery(
            IAlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AlgoTaskRecovery> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SIFSContext>();
            var pendingTasks = await context.AlgoTasks
                .Where(a => a.Status == (int)AlgoTaskStatus.pending && a.DeletedAt == null)
                .Select(a => new { a.Id, a.AlgoModelId })
                .ToListAsync(cancellationToken);

            foreach (var task in pendingTasks)
            {
                if (!task.AlgoModelId.HasValue)
                {
                    _logger.LogWarning("Recovery skipped algo task {TaskId} because AlgoModelId is missing", task.Id);
                    var now = DateTime.UtcNow;
                    await context.AlgoTasks
                        .Where(a => a.Id == task.Id && a.Status == (int)AlgoTaskStatus.pending)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(a => a.Status, (int)AlgoTaskStatus.failed)
                            .SetProperty(a => a.FailureReason, "algorithm model id missing during recovery")
                            .SetProperty(a => a.FinishedAt, now)
                            .SetProperty(a => a.UpdatedAt, now), cancellationToken);
                    continue;
                }

                await _queue.EnqueueAsync(new AlgoTaskQueueItem(task.Id, task.AlgoModelId.Value), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
