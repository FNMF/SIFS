using Microsoft.EntityFrameworkCore;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;

namespace SIFS.Infrastructure
{
    public class AlgoTaskRecoveryService : IHostedService
    {
        private readonly IAlgoTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        public AlgoTaskRecoveryService(
            IAlgoTaskQueue queue,
            IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var appService = scope.ServiceProvider
                .GetRequiredService<SIFSContext>();
            var pendingTasks = await appService.AlgoTasks
                .Where(a => a.Status == (int)AlgoTaskStatus.pending)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            foreach (var taskId in pendingTasks)
            {
                await _queue.EnqueueAsync(taskId);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
