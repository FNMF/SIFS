using Microsoft.EntityFrameworkCore;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Application.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly SIFSContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            SIFSContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<DashboardService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var taskStatusCounts = await GetCurrentTaskStatusCountsAsync();
            var algoStatus = await GetAlgoStatusCountAsync();
            var today = DateTime.Today;

            return new DashboardSummaryDto
            {
                TodayTaskCount = await _context.TaskLists.AsNoTracking().CountAsync(x => x.DeletedAt == null && x.CreatedAt >= today),
                TotalTaskCount = taskStatusCounts.Values.Sum(),
                RunningTaskCount = taskStatusCounts.GetValueOrDefault("running"),
                WaitingTaskCount = taskStatusCounts.GetValueOrDefault("queued") + taskStatusCounts.GetValueOrDefault("pending"),
                FailedTaskCount = taskStatusCounts.GetValueOrDefault("failed"),
                SuccessTaskCount = taskStatusCounts.GetValueOrDefault("done"),
                AlgoTotalCount = algoStatus.Total,
                AlgoEnabledCount = algoStatus.Enabled,
                AlgoOfflineCount = algoStatus.Offline
            };
        }

        public async Task<List<DashboardRecentTaskDto>> GetRecentTasksAsync(int limit, bool failedOnly = false)
        {
            limit = Math.Clamp(limit <= 0 ? 10 : limit, 1, 50);

            var query = _context.TaskLists
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (failedOnly)
                query = query.Where(x => _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.Status == (int)AlgoTaskStatus.failed));

            var taskLists = await query
                .OrderByDescending(x => failedOnly
                    ? _context.AlgoTasks
                        .Where(a => a.TaskId == x.Id && a.DeletedAt == null && a.Status == (int)AlgoTaskStatus.failed)
                        .Max(a => (DateTime?)a.FinishedAt ?? a.UpdatedAt)
                    : x.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return await BuildRecentTasksAsync(taskLists);
        }

        public Task<List<DashboardRecentTaskDto>> GetRecentFailedTasksAsync(int limit)
        {
            return GetRecentTasksAsync(limit, failedOnly: true);
        }

        public async Task<List<DashboardRecentLogDto>> GetRecentLogsAsync(int limit)
        {
            limit = Math.Clamp(limit <= 0 ? 10 : limit, 1, 50);

            return await _context.OperationLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .Select(x => new DashboardRecentLogDto
                {
                    Id = x.Id,
                    ActorId = x.ActorId,
                    ActorUsername = x.ActorUsername,
                    OperationType = x.OperationType,
                    TargetType = x.TargetType,
                    TargetId = x.TargetId,
                    RequestPath = x.RequestPath,
                    Success = x.Success,
                    FailureReason = x.FailureReason,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<DashboardTaskStatusCountDto> GetTaskStatusCountAsync()
        {
            var counts = await GetCurrentTaskStatusCountsAsync();
            return new DashboardTaskStatusCountDto
            {
                Items = counts
                    .OrderBy(x => x.Key)
                    .Select(x => new DashboardStatusCountItemDto
                    {
                        Status = x.Key,
                        Count = x.Value
                    })
                    .ToList()
            };
        }

        public async Task<DashboardAlgoStatusCountDto> GetAlgoStatusCountAsync()
        {
            var algos = await _context.AlgoModels
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .ToListAsync();

            var enabled = algos.Where(x => x.Enabled).ToList();
            var offlineResults = await Task.WhenAll(enabled.Select(IsAlgoOfflineAsync));

            return new DashboardAlgoStatusCountDto
            {
                Total = algos.Count,
                Enabled = enabled.Count,
                Disabled = algos.Count(x => !x.Enabled),
                Offline = offlineResults.Count(x => x)
            };
        }

        private async Task<List<DashboardRecentTaskDto>> BuildRecentTasksAsync(List<TaskList> taskLists)
        {
            if (!taskLists.Any())
                return new List<DashboardRecentTaskDto>();

            var taskIds = taskLists.Select(x => x.Id).ToList();
            var userIds = taskLists.Select(x => x.UserId).Distinct().ToList();

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Account);

            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => taskIds.Contains(x.TaskId) && x.DeletedAt == null)
                .ToListAsync();

            return taskLists.Select(task =>
            {
                var current = algoTasks.Where(x => x.TaskId == task.Id).ToList();

                return new DashboardRecentTaskDto
                {
                    TaskId = task.Id,
                    CreatedByUserId = task.UserId,
                    CreatedByUsername = users.TryGetValue(task.UserId, out var username) ? username : null,
                    AlgorithmName = GetAlgorithmSummary(current),
                    Status = GetCurrentStatus(task, current),
                    CreatedAt = task.CreatedAt,
                    StartedAt = current.Where(x => x.StartedAt.HasValue).Select(x => x.StartedAt!.Value).DefaultIfEmpty().Min() is var started && started != default ? started : null,
                    FinishedAt = GetFinishedAt(current),
                    FailureReason = current.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FailureReason))?.FailureReason
                };
            }).ToList();
        }

        private async Task<Dictionary<string, int>> GetCurrentTaskStatusCountsAsync()
        {
            var taskLists = await _context.TaskLists
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Select(x => new { x.Id, x.DeletedAt })
                .ToListAsync();

            if (!taskLists.Any())
                return new Dictionary<string, int>();

            var taskIds = taskLists.Select(x => x.Id).ToList();
            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => taskIds.Contains(x.TaskId) && x.DeletedAt == null)
                .ToListAsync();

            return taskLists
                .GroupJoin(
                    algoTasks,
                    task => task.Id,
                    algo => algo.TaskId,
                    (task, algos) => GetCurrentStatus(task.DeletedAt != null, algos.ToList()))
                .GroupBy(status => status)
                .ToDictionary(x => x.Key, x => x.Count());
        }

        private async Task<bool> IsAlgoOfflineAsync(AlgoModel algo)
        {
            if (!Uri.TryCreate(algo.ApiUrl, UriKind.Absolute, out var uri))
                return true;

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(uri, timeout.Token);
                return !response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Algorithm ping failed. AlgoId={AlgoId}, Url={Url}", algo.Id, algo.ApiUrl);
                return true;
            }
        }

        private static string GetCurrentStatus(TaskList task, List<AlgoTask> algoTasks)
        {
            return GetCurrentStatus(task.DeletedAt != null, algoTasks);
        }

        private static string GetCurrentStatus(bool isDeleted, List<AlgoTask> algoTasks)
        {
            if (isDeleted)
                return "deleted";
            if (!algoTasks.Any())
                return "pending";
            if (algoTasks.All(x => x.Status == (int)AlgoTaskStatus.deleted))
                return "deleted";
            if (algoTasks.All(x => x.Status == (int)AlgoTaskStatus.canceled))
                return "canceled";
            if (algoTasks.Any(x => x.Status == (int)AlgoTaskStatus.running))
                return "running";
            if (algoTasks.Any(x => x.Status == (int)AlgoTaskStatus.failed))
                return "failed";
            if (algoTasks.All(x => x.Status == (int)AlgoTaskStatus.done))
                return "done";
            return "queued";
        }

        private static DateTime? GetFinishedAt(List<AlgoTask> algoTasks)
        {
            var finished = algoTasks
                .Where(x => x.FinishedAt.HasValue)
                .Select(x => x.FinishedAt!.Value)
                .ToList();

            return finished.Count == algoTasks.Count && finished.Count > 0 ? finished.Max() : null;
        }

        private static string? GetAlgorithmSummary(List<AlgoTask> algoTasks)
        {
            var names = algoTasks
                .Select(x => x.AlgoName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            return names.Count == 0 ? null : string.Join(",", names);
        }
    }
}
