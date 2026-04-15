using Microsoft.EntityFrameworkCore;
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

            // 取 TaskTypeMap
            var taskTypeMap = await _context.TaskTypeMaps
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TaskId == id);

            if (taskTypeMap == null)
                throw new Exception($"AlgoTask {id} 没有对应的 TaskTypeMap");

            // 取 AlgoType
            var algoType = await _context.AlgoTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskTypeMap.TypeId);

            if (algoType == null)
                throw new Exception($"TypeId {taskTypeMap.TypeId} 未找到对应的 AlgoType");

            // Map 聚合
            var taskAggregate = MapToAggregate(entity, taskList, localFile, algoType);

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
            {
                return new List<AlgoReadDto>();
            }

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            var localFiles = await _context.Localfiles
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.AlgoTaskId))
                .ToListAsync();

            var taskTypeMaps = await _context.TaskTypeMaps
                .AsNoTracking()
                .Where(x => algoTaskIds.Contains(x.TaskId))
                .ToListAsync();

            var typeIds = taskTypeMaps
                .Select(x => x.TypeId)
                .Distinct()
                .ToList();

            var algoTypes = await _context.AlgoTypes
                .AsNoTracking()
                .Where(x => typeIds.Contains(x.Id))
                .ToListAsync();

            var fileDict = localFiles
                .GroupBy(x => x.AlgoTaskId)
                .ToDictionary(x => x.Key, x => x.First().UrlLocal);

            var typeMapDict = taskTypeMaps
                .GroupBy(x => x.TaskId)
                .ToDictionary(x => x.Key, x => x.First().TypeId);

            var typeDict = algoTypes.ToDictionary(x => x.Id, x => x.Name);

            return algoTasks.Select(x =>
            {
                fileDict.TryGetValue(x.Id, out var url);
                typeMapDict.TryGetValue(x.Id, out var typeId);

                string? typeName = null;
                if (typeId != 0 && typeDict.TryGetValue(typeId, out var name))
                {
                    typeName = name;
                }

                return new AlgoReadDto
                {
                    Guid = x.Id,
                    Url = string.IsNullOrWhiteSpace(url) ? string.Empty : _fileUrlBuilder.ToPythonUrl(url),
                    Type = typeName ?? string.Empty,
                    Status = x.Status,
                    UpdatedAt = x.UpdatedAt
                };
            }).ToList();
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
            Localfile localFile,
            AlgoType algoType)
        {
            // 枚举类型转换
            var type = Enum.Parse<AiServiceType>(algoType.Name, true);

            // 构建 TaskItem
            var task = new TaskItem(entity.TaskId, localFile.UrlLocal, type, taskList.Level);

            // 回填基础字段
            typeof(TaskItem).GetProperty("Id")!.SetValue(task, entity.Id);
            typeof(TaskItem).GetProperty("CreatedAt")!.SetValue(task, entity.CreatedAt);
            typeof(TaskItem).GetProperty("UpdatedAt")!.SetValue(task, entity.UpdatedAt);

            // 状态机映射
            switch ((AlgoTaskStatus)entity.Status)
            {
                case AlgoTaskStatus.pending:
                    // 默认就是 pending
                    break;
                case AlgoTaskStatus.running:
                    task.MarkAsRunning();
                    break;
                case AlgoTaskStatus.done:
                    task.MarkAsDone(new DetectionResult()); // 占位
                    break;
                case AlgoTaskStatus.failed:
                    task.MarkAsFailed();
                    break;
            }

            return task;
        }
    }
}
