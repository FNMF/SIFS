using Microsoft.EntityFrameworkCore;
using SIFS.Application.DetectionTaskApp;
using SIFS.Domain.Entities;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Extensions;
using SIFS.Shared.Results;
using System;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskListRepository : ITaskListRepository
    {
        private readonly SIFSContext _context;
        private readonly IFileUrlBuilder _fileUrlBuilder;
        public TaskListRepository(SIFSContext context, IFileUrlBuilder fileUrlBuilder)
        {
            _context = context;
            _fileUrlBuilder = fileUrlBuilder;
        }
        public async Task<Result<TaskList>> GetTaskListByIdAsync(Guid id)
        {
            var taskList = await _context.TaskLists
                .AsNoTracking()
                .Where(t => t.DeletedAt == null)
                .FirstOrDefaultAsync(t => t.Id == id);
            return taskList != null
                ? Result<TaskList>.Success(taskList)
                : Result<TaskList>.Fail(ResultCode.NotFound, "任务列表记录不存在");
        }
        public async Task<Result<DetectionTask>> GetDetectionTaskAggregateByGuidAsync(Guid id)
        {
            var taskListResult = await GetTaskListByIdAsync(id);
            if (!taskListResult.IsSuccess)
            {
                return Result<DetectionTask>.Fail(taskListResult.Code, taskListResult.Message);
            }

            var taskList = taskListResult.Data;

            // 查 AlgoTasks
            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(t => t.TaskId == taskList.Id)
                .ToListAsync();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            // 查 LocalFiles
            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(f => algoTaskIds.Contains(f.AlgoTaskId))
                .ToListAsync();

            var detectionTask = MapToAggregate(
                taskList,
                algoTasks,
                localFiles);

            return Result<DetectionTask>.Success(detectionTask);
        }
        public async Task<List<DetectionTaskReadDto>> GetAllReadDtosByUserIdAsync(Guid userId)
        {
            var taskLists = await _context.TaskLists
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return await BuildReadDtosAsync(taskLists);
        }
        public async Task<List<DetectionTaskReadDto>> GetAllReadDtosAsync()
        {
            var taskLists = await _context.TaskLists
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return await BuildReadDtosAsync(taskLists);
        }
        private async Task<List<DetectionTaskReadDto>> BuildReadDtosAsync(List<TaskList> taskLists)
        {
            if (!taskLists.Any())
                return new List<DetectionTaskReadDto>();

            var taskIds = taskLists.Select(x => x.Id).ToList();

            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => taskIds.Contains(x.TaskId))
                .ToListAsync();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();

            var taskImageMap = algoTasks
    .Join(localFiles,
        algo => algo.Id,
        file => file.AlgoTaskId,
        (algo, file) => new
        {
            TaskId = algo.TaskId,
            Url = string.IsNullOrWhiteSpace(file.UrlLocal)
                ? string.Empty
                : _fileUrlBuilder.ToAbsoluteUrl(file.UrlLocal),
            Sid = file.Sid
        })
    .GroupBy(x => x.TaskId)
    .ToDictionary(
        g => g.Key,
        g => g
            .OrderBy(x => x.Sid)
            .Select(x => x.Url)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList()
    );

            return taskLists.Select(task =>
            {
                var currentAlgoTasks = algoTasks.Where(x => x.TaskId == task.Id).ToList();

                var subTaskCount = currentAlgoTasks.Count;
                var completedSubTaskCount = currentAlgoTasks.Count(x => x.Status == (int)AlgoTaskStatus.done);
                var completion = subTaskCount == 0
                    ? 0m
                    : Math.Round((decimal)completedSubTaskCount / subTaskCount, 4);

                taskImageMap.TryGetValue(task.Id, out var imageUrls);
                imageUrls ??= new List<string>();

                return new DetectionTaskReadDto
                {
                    Guid = task.Id,
                    SubTaskCount = subTaskCount,
                    CompletedSubTaskCount = completedSubTaskCount,
                    Completion = completion,
                    PreviewImageUrl = imageUrls.FirstOrDefault() ?? string.Empty,
                    ImageCount = imageUrls.Count,
                    Level = task.Level,
                    UpdatedAt = task.UpdatedAt
                };
            }).ToList();
        }
        public async Task<List<string>> GetImageUrlsByTaskIdAsync(Guid taskId)
        {
            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .ToListAsync();

            if (!algoTasks.Any())
                return new List<string>();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .OrderBy(x => x.Sid)
                .ToListAsync();

            return localFiles
                .Select(x => _fileUrlBuilder.ToAbsoluteUrl(x.UrlLocal))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public async Task<(int CompletedCount, int TotalCount, bool IsCompleted)> RefreshProgressFromSubTasksAsync(Guid taskId)
        {
            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => x.TaskId == taskId && x.DeletedAt == null)
                .Select(x => x.Status)
                .ToListAsync();

            var totalCount = algoTasks.Count;
            var completedCount = algoTasks.Count(x => x == (int)AlgoTaskStatus.done);
            var now = DateTime.UtcNow;

            await _context.TaskLists
                .Where(x => x.Id == taskId && x.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, completedCount)
                    .SetProperty(x => x.UpdatedAt, now));

            return (completedCount, totalCount, totalCount > 0 && completedCount == totalCount);
        }

        public async Task InsertAsync(TaskList taskList)
        {
            await _context.TaskLists.AddAsync(taskList);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(TaskList taskList)
        {
            var local = _context.TaskLists.Local.FirstOrDefault(x => x.Id == taskList.Id);

            if (local != null)
            {
                _context.Entry(local).CurrentValues.SetValues(taskList);
            }
            else
            {
                _context.TaskLists.Attach(taskList);
                _context.Entry(taskList).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
        private DetectionTask MapToAggregate(
            TaskList taskList,
            List<AlgoTask> algoTasks,
            List<Localfile> localFiles)
        {
            //  URL（按顺序）
            var urls = localFiles
                .OrderBy(f => f.Sid)
                .Select(f => f.UrlLocal)
                .ToList();

            var algorithms = algoTasks
                .Where(x => x.DeletedAt == null)
                .Select(x => new AlgorithmRef
                {
                    AlgoModelId = x.AlgoModelId,
                    AlgoName = x.AlgoName ?? string.Empty,
                    AlgoApiUrl = x.AlgoApiUrl ?? string.Empty
                })
                .GroupBy(x => x.AlgoModelId?.ToString() ?? x.AlgoName)
                .Select(x => x.First())
                .ToList();

            var detectionTask = new DetectionTask(taskList.UserId, urls, algorithms, taskList.Level);

            // 回填基础字段（重要）
            typeof(DetectionTask).GetProperty("Id")!
                .SetValue(detectionTask, taskList.Id);

            typeof(DetectionTask).GetProperty("CreatedAt")!
                .SetValue(detectionTask, taskList.CreatedAt);

            typeof(DetectionTask).GetProperty("UpdatedAt")!
                .SetValue(detectionTask, taskList.UpdatedAt);

            var completedCount = algoTasks.Count(x => x.Status == (int)AlgoTaskStatus.done);
            detectionTask.SetCompletedSubTaskCount(completedCount);

            return detectionTask;
        }
    }
}
