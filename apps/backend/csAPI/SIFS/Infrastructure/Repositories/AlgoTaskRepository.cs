using Microsoft.EntityFrameworkCore;
using SIFS.Application.AlgoTaskApp;
using SIFS.Application.DetectionTaskApp;
using SIFS.Domain.Entities;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Extensions;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class AlgoTaskRepository: IAlgoTaskRepository
    {
        private readonly SIFSContext _context;
        private readonly IFileUrlBuilder _fileUrlBuilder;
        public AlgoTaskRepository(SIFSContext context, IFileUrlBuilder fileUrlBuilder)
        {
            _context = context;
            _fileUrlBuilder = fileUrlBuilder;
        }
        public async Task<Result<AlgoTask>> GetTaskByIdAsync(Guid id)
        {
            var algoTask = await _context.AlgoTasks
                .AsNoTracking()
                .Where(t => t.DeletedAt == null)
                .FirstOrDefaultAsync(t => t.Id == id);
            return algoTask != null
                ? Result<AlgoTask>.Success(algoTask)
                : Result<AlgoTask>.Fail(ResultCode.NotFound, "算法任务记录不存在");
        }
        public async Task<Result<TaskItem>> GetAggregateByGuidAsync(Guid id)
        {
            // 取 AlgoTask 基础实体
            var entity = await _context.AlgoTasks
                .AsNoTracking()
                .Where(t => t.DeletedAt == null)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                return Result<TaskItem>.Fail(ResultCode.NotFound, "AlgoTask不存在");

            // 取 TaskList
            var taskList = await _context.TaskLists
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == entity.TaskId);
            if (taskList == null)
                return Result<TaskItem>.Fail(ResultCode.NotFound, "父任务不存在");

            // 取 Localfile
            var localFile = await _context.Localfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.AlgoTaskId == id);

            if (localFile == null)
                throw new Exception($"AlgoTask {id} 没有对应的 Localfile");

            // Map 聚合
            var taskAggregate = MapToAggregate(entity, taskList, localFile);

            return Result<TaskItem>.Success(taskAggregate);
        }
        public async Task<List<AlgoReadDto>> GetAllReadDtosByTaskIdAsync(Guid taskId)
        {
            var algoTasks = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            if (!algoTasks.Any())
                return new List<AlgoReadDto>();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            var taskList = await _context.TaskLists
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (taskList == null)
                throw new Exception("父任务不存在");

            // 这里从 Resultfiles 取结果图
            var resultFiles = await _context.ResultFiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();

            var resultFileDict = resultFiles
                .GroupBy(x => x.AlgoTaskId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .Select(r => r.MaskLocalUrl)
                        .FirstOrDefault() ?? string.Empty
                );

            return algoTasks.Select(x =>
            {
                resultFileDict.TryGetValue(x.Id, out var url);

                return new AlgoReadDto
                {
                    Guid = x.Id,
                    Url = string.IsNullOrWhiteSpace(url)
                            ? string.Empty
                            : _fileUrlBuilder.ToPythonUrl(url),
                    Type = x.AlgoName ?? string.Empty,
                    Status = x.Status,
                    Level = taskList.Level,
                    FailureReason = x.FailureReason,
                    UpdatedAt = x.UpdatedAt
                };
            }).ToList();
        }
        public async Task<AlgoTaskDetailDto?> GetDetailDtoByIdAsync(Guid algoTaskId, Guid userId, bool canViewAllTasks = false)
        {
            var algoTask = await _context.AlgoTasks
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .FirstOrDefaultAsync(x => x.Id == algoTaskId);

            if (algoTask == null)
                return null;

            var taskList = await _context.TaskLists
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == algoTask.TaskId && x.DeletedAt == null);

            if (taskList == null || (!canViewAllTasks && taskList.UserId != userId))
                return null;

            var localFile = await _context.Localfiles
                .AsNoTracking()
                .Where(x => x.AlgoTaskId == algoTaskId)
                .OrderBy(x => x.Sid)
                .FirstOrDefaultAsync();

            var resultFile = await _context.ResultFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AlgoTaskId == algoTaskId);

            return new AlgoTaskDetailDto
            {
                Guid = algoTask.Id,
                OriginImageUrl = localFile == null
                    ? string.Empty
                    : _fileUrlBuilder.ToAbsoluteUrl(localFile.UrlLocal),

                MaskUrl = string.IsNullOrWhiteSpace(resultFile?.MaskLocalUrl)
                    ? string.Empty
                    : _fileUrlBuilder.ToPythonUrl(resultFile!.MaskLocalUrl!),

                Type = algoTask.AlgoName ?? string.Empty,
                Status = algoTask.Status,
                StatusText = algoTask.Status.ToString(),
                Level = taskList.Level,
                FailureReason = algoTask.FailureReason,
                AlgoApiUrl = algoTask.AlgoApiUrl,
                IsFake = resultFile?.IsFake,
                Confidence = resultFile?.Confidence == null
                    ? null
                    : Convert.ToDecimal(resultFile.Confidence.Value)
            };
        }

        public async Task<bool> IsRunningAsync(Guid id)
        {
            return await _context.AlgoTasks
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.Status == (int)AlgoTaskStatus.running && x.DeletedAt == null);
        }

        public async Task<bool> TryMarkRunningAsync(Guid id)
        {
            var now = DateTime.UtcNow;
            var affected = await _context.AlgoTasks
                .Where(x => x.Id == id && x.Status == (int)AlgoTaskStatus.pending && x.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, (int)AlgoTaskStatus.running)
                    .SetProperty(x => x.StartedAt, x => x.StartedAt ?? now)
                    .SetProperty(x => x.UpdatedAt, now));

            return affected == 1;
        }

        public async Task<bool> TryMarkDoneAsync(Guid id)
        {
            var now = DateTime.UtcNow;
            var affected = await _context.AlgoTasks
                .Where(x => x.Id == id && x.Status == (int)AlgoTaskStatus.running && x.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, (int)AlgoTaskStatus.done)
                    .SetProperty(x => x.FinishedAt, now)
                    .SetProperty(x => x.UpdatedAt, now));

            return affected == 1;
        }

        public async Task<bool> TryMarkFailedAsync(Guid id, string failureReason)
        {
            var now = DateTime.UtcNow;
            var safeReason = string.IsNullOrWhiteSpace(failureReason)
                ? "algorithm invocation failed"
                : failureReason;

            var affected = await _context.AlgoTasks
                .Where(x => x.Id == id && x.Status == (int)AlgoTaskStatus.running && x.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, (int)AlgoTaskStatus.failed)
                    .SetProperty(x => x.FailureReason, safeReason)
                    .SetProperty(x => x.FinishedAt, now)
                    .SetProperty(x => x.UpdatedAt, now));

            return affected == 1;
        }

        public async Task InsertAsync(AlgoTask algoTask)
        {
            await _context.AlgoTasks.AddAsync(algoTask);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(AlgoTask algoTask)
        {
            var local = _context.AlgoTasks.Local.FirstOrDefault(x => x.Id == algoTask.Id);

            if (local != null)
            {
                // 已经有同 Id 的实体被跟踪，直接把值拷进去
                _context.Entry(local).CurrentValues.SetValues(algoTask);
            }
            else
            {
                // 当前没跟踪，就附加并标记修改
                _context.AlgoTasks.Attach(algoTask);
                _context.Entry(algoTask).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
        private TaskItem MapToAggregate(
            AlgoTask entity,
            TaskList taskList,
            Localfile localFile)
        {
            var task = new TaskItem(entity.TaskId, localFile.UrlLocal, new AlgorithmRef
            {
                AlgoModelId = entity.AlgoModelId,
                AlgoName = entity.AlgoName ?? string.Empty,
                AlgoApiUrl = entity.AlgoApiUrl ?? string.Empty
            }, taskList.Level);

            // 回填基础字段
            typeof(TaskItem).GetProperty("Id")!.SetValue(task, entity.Id);
            typeof(TaskItem).GetProperty("CreatedAt")!.SetValue(task, entity.CreatedAt);
            typeof(TaskItem).GetProperty("UpdatedAt")!.SetValue(task, entity.UpdatedAt);
            typeof(TaskItem).GetProperty("AlgoModelId")!.SetValue(task, entity.AlgoModelId);
            typeof(TaskItem).GetProperty("AlgoName")!.SetValue(task, entity.AlgoName);
            typeof(TaskItem).GetProperty("AlgoApiUrl")!.SetValue(task, entity.AlgoApiUrl);
            typeof(TaskItem).GetProperty("FailureReason")!.SetValue(task, entity.FailureReason);
            typeof(TaskItem).GetProperty("StartedAt")!.SetValue(task, entity.StartedAt);
            typeof(TaskItem).GetProperty("FinishedAt")!.SetValue(task, entity.FinishedAt);
            typeof(TaskItem).GetProperty("DeletedAt")!.SetValue(task, entity.DeletedAt);

            if (Enum.IsDefined(typeof(AlgoTaskStatus), entity.Status))
                typeof(TaskItem).GetProperty("Status")!.SetValue(task, (AlgoTaskStatus)entity.Status);

            return task;
        }
    }
}
