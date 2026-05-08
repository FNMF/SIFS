using Microsoft.EntityFrameworkCore;
using SIFS.Application.TaskManagement;
using SIFS.Domain.Enum;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Extensions;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskManagementRepository : ITaskManagementRepository
    {
        private readonly SIFSContext _context;
        private readonly IFileUrlBuilder _fileUrlBuilder;

        public TaskManagementRepository(SIFSContext context, IFileUrlBuilder fileUrlBuilder)
        {
            _context = context;
            _fileUrlBuilder = fileUrlBuilder;
        }

        public async Task<Paged<TaskManagementListItemDto>> QueryAsync(TaskManagementQuery query, Guid? restrictToUserId = null)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var taskLists = _context.TaskLists
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (restrictToUserId.HasValue)
                taskLists = taskLists.Where(x => x.UserId == restrictToUserId.Value);
            else if (query.UserId.HasValue)
                taskLists = taskLists.Where(x => x.UserId == query.UserId.Value);

            if (query.StartTime.HasValue)
                taskLists = taskLists.Where(x => x.CreatedAt >= query.StartTime.Value);

            if (query.EndTime.HasValue)
                taskLists = taskLists.Where(x => x.CreatedAt <= query.EndTime.Value);

            if (query.AlgorithmId.HasValue)
                taskLists = taskLists.Where(x => _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.AlgoModelId == query.AlgorithmId.Value));

            if (!string.IsNullOrWhiteSpace(query.AlgorithmName))
            {
                var algorithmName = query.AlgorithmName.Trim();
                taskLists = taskLists.Where(x => _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.AlgoName != null && a.AlgoName.Contains(algorithmName)));
            }

            if (query.Status.HasValue)
                taskLists = taskLists.Where(x => _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.Status == query.Status.Value));

            if (query.Failed.HasValue)
            {
                taskLists = query.Failed.Value
                    ? taskLists.Where(x => _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.Status == (int)AlgoTaskStatus.failed))
                    : taskLists.Where(x => !_context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.Status == (int)AlgoTaskStatus.failed));
            }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                taskLists = taskLists.Where(x =>
                    x.Id.ToString().Contains(keyword) ||
                    _context.Users.Any(u => u.Id == x.UserId && u.Account.Contains(keyword)) ||
                    _context.AlgoTasks.Any(a => a.TaskId == x.Id && a.DeletedAt == null && a.AlgoName != null && a.AlgoName.Contains(keyword)));
            }

            var total = await taskLists.CountAsync();
            var pageItems = await taskLists
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Paged<TaskManagementListItemDto>
            {
                Data = await BuildListItemsAsync(pageItems),
                Total = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<TaskManagementDetailDto?> GetDetailAsync(Guid taskId, Guid? restrictToUserId = null, bool includeDeleted = false)
        {
            var taskList = await _context.TaskLists
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (taskList == null || (!includeDeleted && taskList.DeletedAt != null))
                return null;

            if (restrictToUserId.HasValue && taskList.UserId != restrictToUserId.Value)
                return null;

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == taskList.UserId)
                .ToDictionaryAsync(x => x.Id, x => x.Account);

            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => x.TaskId == taskList.Id && (includeDeleted || x.DeletedAt == null))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();
            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .OrderBy(x => x.Sid)
                .ToListAsync();

            var resultFiles = await _context.ResultFiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();

            var originalPaths = localFiles
                .Select(x => _fileUrlBuilder.ToAbsoluteUrl(x.UrlLocal))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var resultPaths = resultFiles
                .Select(x => string.IsNullOrWhiteSpace(x.MaskLocalUrl) ? string.Empty : _fileUrlBuilder.ToPythonUrl(x.MaskLocalUrl!))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var localByTask = localFiles
                .GroupBy(x => x.AlgoTaskId)
                .ToDictionary(x => x.Key, x => x.OrderBy(f => f.Sid).First());

            var resultByTask = resultFiles
                .GroupBy(x => x.AlgoTaskId)
                .ToDictionary(x => x.Key, x => x.First());
            var subTasks = algoTasks.Select(x =>
            {
                localByTask.TryGetValue(x.Id, out var localFile);
                resultByTask.TryGetValue(x.Id, out var resultFile);

                return new TaskManagementSubTaskDto
                {
                    TaskId = x.Id,
                    AlgorithmId = x.AlgoModelId,
                    AlgorithmName = x.AlgoName,
                    Status = x.Status,
                    StatusText = GetStatusText(x.Status),
                    OriginalImagePath = localFile == null ? null : _fileUrlBuilder.ToAbsoluteUrl(localFile.UrlLocal),
                    ResultPath = string.IsNullOrWhiteSpace(resultFile?.MaskLocalUrl) ? null : _fileUrlBuilder.ToPythonUrl(resultFile!.MaskLocalUrl!),
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    StartedAt = x.StartedAt,
                    FinishedAt = x.FinishedAt,
                    Duration = GetDurationSeconds(x.StartedAt, x.FinishedAt),
                    FailureReason = x.FailureReason
                };
            }).ToList();

            var startedAt = algoTasks.Where(x => x.StartedAt.HasValue).Select(x => x.StartedAt!.Value).DefaultIfEmpty().Min();
            var finishedCandidates = algoTasks.Where(x => x.FinishedAt.HasValue).Select(x => x.FinishedAt!.Value).ToList();
            DateTime? finishedAt = finishedCandidates.Count == algoTasks.Count && finishedCandidates.Count > 0
                ? finishedCandidates.Max()
                : null;

            return new TaskManagementDetailDto
            {
                TaskId = taskList.Id,
                CreatedByUserId = taskList.UserId,
                CreatedByUsername = users.TryGetValue(taskList.UserId, out var username) ? username : null,
                AlgorithmId = algoTasks.Select(x => x.AlgoModelId).Distinct().Count() == 1 ? algoTasks.FirstOrDefault()?.AlgoModelId : null,
                AlgorithmName = GetAlgorithmSummary(algoTasks),
                CurrentStatus = GetCurrentStatus(taskList, algoTasks),
                OriginalImagePath = originalPaths.FirstOrDefault(),
                OriginalImagePaths = originalPaths,
                ResultPath = resultPaths.FirstOrDefault(),
                ResultPaths = resultPaths,
                CreatedAt = taskList.CreatedAt,
                UpdatedAt = taskList.UpdatedAt,
                StartedAt = startedAt == default ? null : startedAt,
                FinishedAt = finishedAt,
                Duration = GetDurationSeconds(startedAt == default ? null : startedAt, finishedAt),
                FailureReason = algoTasks.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FailureReason))?.FailureReason,
                SubTasks = subTasks
            };
        }

        public async Task<List<TaskStatusFlowItemDto>?> GetStatusFlowAsync(Guid taskId, Guid? restrictToUserId = null)
        {
            var detail = await GetDetailAsync(taskId, restrictToUserId, includeDeleted: true);
            if (detail == null)
                return null;

            var flow = new List<TaskStatusFlowItemDto>
            {
                new() { Status = "created", CreatedAt = detail.CreatedAt }
            };

            if (detail.StartedAt.HasValue)
                flow.Add(new TaskStatusFlowItemDto { Status = "started", CreatedAt = detail.StartedAt.Value });

            if (detail.FinishedAt.HasValue)
            {
                var finalStatus = detail.CurrentStatus == "failed" ? "failed" : "finished";
                flow.Add(new TaskStatusFlowItemDto
                {
                    Status = finalStatus,
                    CreatedAt = detail.FinishedAt.Value,
                    Reason = detail.FailureReason
                });
            }
            else if (detail.CurrentStatus is "canceled" or "deleted")
            {
                flow.Add(new TaskStatusFlowItemDto
                {
                    Status = detail.CurrentStatus,
                    CreatedAt = detail.UpdatedAt,
                    Reason = detail.FailureReason
                });
            }

            return flow;
        }

        public async Task<bool> ExistsForUserAsync(Guid taskId, Guid userId)
        {
            return await _context.TaskLists.AnyAsync(x => x.Id == taskId && x.UserId == userId && x.DeletedAt == null);
        }

        public async Task CancelAsync(Guid taskId, string reason)
        {
            var now = DateTime.UtcNow;
            var taskList = await _context.TaskLists.FirstAsync(x => x.Id == taskId);
            var algoTasks = await _context.AlgoTasks
                .Where(x => x.TaskId == taskId && x.DeletedAt == null)
                .ToListAsync();

            foreach (var task in algoTasks.Where(x => x.Status is (int)AlgoTaskStatus.pending or (int)AlgoTaskStatus.running))
            {
                task.Status = (int)AlgoTaskStatus.canceled;
                task.FailureReason = reason;
                task.FinishedAt = now;
                task.UpdatedAt = now;
            }

            taskList.UpdatedAt = now;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid taskId, string reason)
        {
            var now = DateTime.UtcNow;
            var taskList = await _context.TaskLists.FirstAsync(x => x.Id == taskId);
            var algoTasks = await _context.AlgoTasks
                .Where(x => x.TaskId == taskId)
                .ToListAsync();

            taskList.DeletedAt = now;
            taskList.UpdatedAt = now;

            foreach (var task in algoTasks)
            {
                task.Status = (int)AlgoTaskStatus.deleted;
                task.FailureReason = string.IsNullOrWhiteSpace(task.FailureReason) ? reason : task.FailureReason;
                task.DeletedAt = now;
                task.FinishedAt ??= now;
                task.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<(Guid NewTaskId, List<AlgoTaskQueueItem> AlgoTasks)> RetryAsync(Guid taskId, Dictionary<Guid, AlgorithmEndpointResolution> algorithmEndpoints)
        {
            var original = await _context.TaskLists.AsNoTracking().FirstAsync(x => x.Id == taskId);
            var originalAlgoTasks = await _context.AlgoTasks.AsNoTracking()
                .Where(x => x.TaskId == taskId && x.DeletedAt == null)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var originalAlgoTaskIds = originalAlgoTasks.Select(x => x.Id).ToList();
            var originalLocalFiles = await _context.Localfiles.AsNoTracking()
                .Where(x => originalAlgoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();
            var now = DateTime.UtcNow;
            var newTaskId = UuidV7.NewUuidV7();
            var newAlgoTasks = new List<AlgoTaskQueueItem>();

            _context.TaskLists.Add(new TaskList
            {
                Id = newTaskId,
                UserId = original.UserId,
                Level = original.Level,
                Status = 0,
                CreatedAt = now,
                UpdatedAt = now
            });

            foreach (var originalAlgoTask in originalAlgoTasks)
            {
                if (!algorithmEndpoints.TryGetValue(originalAlgoTask.Id, out var endpoint))
                    continue;

                var newAlgoTaskId = UuidV7.NewUuidV7();
                newAlgoTasks.Add(new AlgoTaskQueueItem(newAlgoTaskId, endpoint.AlgoModelId!.Value));

                _context.AlgoTasks.Add(new AlgoTask
                {
                    Id = newAlgoTaskId,
                    TaskId = newTaskId,
                    Status = (int)AlgoTaskStatus.pending,
                    AlgoModelId = endpoint.AlgoModelId,
                    AlgoName = endpoint.AlgoName,
                    AlgoApiUrl = endpoint.ApiUrl,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                var localFile = originalLocalFiles.FirstOrDefault(x => x.AlgoTaskId == originalAlgoTask.Id);
                if (localFile != null)
                {
                    _context.Localfiles.Add(new Localfile
                    {
                        Id = UuidV7.NewUuidV7(),
                        AlgoTaskId = newAlgoTaskId,
                        UrlLocal = localFile.UrlLocal,
                        Sid = localFile.Sid,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return (newTaskId, newAlgoTasks);
        }

        private async Task<List<TaskManagementListItemDto>> BuildListItemsAsync(List<TaskList> taskLists)
        {
            if (!taskLists.Any())
                return new List<TaskManagementListItemDto>();

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

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();
            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();

            var previewByTask = algoTasks
                .Join(localFiles, task => task.Id, file => file.AlgoTaskId, (task, file) => new { task.TaskId, file.UrlLocal, file.Sid })
                .GroupBy(x => x.TaskId)
                .ToDictionary(x => x.Key, x => x.OrderBy(f => f.Sid).Select(f => _fileUrlBuilder.ToAbsoluteUrl(f.UrlLocal)).FirstOrDefault());

            return taskLists.Select(task =>
            {
                var currentAlgoTasks = algoTasks.Where(x => x.TaskId == task.Id).ToList();
                var startedAt = currentAlgoTasks.Where(x => x.StartedAt.HasValue).Select(x => x.StartedAt!.Value).DefaultIfEmpty().Min();
                var finishedCandidates = currentAlgoTasks.Where(x => x.FinishedAt.HasValue).Select(x => x.FinishedAt!.Value).ToList();
                DateTime? finishedAt = finishedCandidates.Count == currentAlgoTasks.Count && finishedCandidates.Count > 0
                    ? finishedCandidates.Max()
                    : null;

                return new TaskManagementListItemDto
                {
                    TaskId = task.Id,
                    CreatedByUserId = task.UserId,
                    CreatedByUsername = users.TryGetValue(task.UserId, out var username) ? username : null,
                    CurrentStatus = GetCurrentStatus(task, currentAlgoTasks),
                    SubTaskCount = currentAlgoTasks.Count,
                    CompletedSubTaskCount = currentAlgoTasks.Count(x => x.Status == (int)AlgoTaskStatus.done),
                    FailedSubTaskCount = currentAlgoTasks.Count(x => x.Status == (int)AlgoTaskStatus.failed),
                    AlgorithmName = GetAlgorithmSummary(currentAlgoTasks),
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    StartedAt = startedAt == default ? null : startedAt,
                    FinishedAt = finishedAt,
                    Duration = GetDurationSeconds(startedAt == default ? null : startedAt, finishedAt),
                    FailureReason = currentAlgoTasks.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FailureReason))?.FailureReason,
                    PreviewImageUrl = previewByTask.TryGetValue(task.Id, out var preview) ? preview : null
                };
            }).ToList();
        }

        private static string GetCurrentStatus(TaskList taskList, List<AlgoTask> algoTasks)
        {
            if (taskList.DeletedAt != null)
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
            return "pending";
        }

        private static string GetStatusText(int status)
        {
            return Enum.IsDefined(typeof(AlgoTaskStatus), status)
                ? ((AlgoTaskStatus)status).ToString()
                : status.ToString();
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

        private static double? GetDurationSeconds(DateTime? startedAt, DateTime? finishedAt)
        {
            return startedAt.HasValue && finishedAt.HasValue
                ? Math.Round((finishedAt.Value - startedAt.Value).TotalSeconds, 3)
                : null;
        }
    }
}
