using Microsoft.EntityFrameworkCore;
using SIFS.Domain.Entities;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskListRepository : ITaskListRepository
    {
        private readonly SIFSContext _context;
        public TaskListRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task<Result<TaskList>> GetTaskListByIdAsync(Guid id)
        {
            var taskList = await _context.TaskLists.FirstOrDefaultAsync(t => t.Id == id);
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
                .Where(t => t.TaskId == taskList.Id)
                .ToListAsync();

            var algoTaskIds = algoTasks.Select(x => x.Id).ToList();

            // 查 LocalFiles
            var localFiles = await _context.Localfiles
                .Where(f => algoTaskIds.Contains(f.AlgoTaskId))
                .ToListAsync();

            // 查 TaskTypeMap（所有子任务）
            var taskTypeMaps = await _context.TaskTypeMaps
                .Where(m => algoTaskIds.Contains(m.TaskId))
                .ToListAsync();

            var typeIds = taskTypeMaps.Select(x => x.TypeId).Distinct().ToList();

            // 查 AlgoType
            var algoTypes = await _context.AlgoTypes
                .Where(t => typeIds.Contains(t.Id))
                .ToListAsync();

            // 5️⃣ 组装 Aggregate
            var detectionTask = MapToAggregate(
                taskList,
                algoTasks,
                localFiles,
                taskTypeMaps,
                algoTypes);

            return Result<DetectionTask>.Success(detectionTask);
        }
        public async Task InsertAsync(TaskList taskList)
        {
            await _context.TaskLists.AddAsync(taskList);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(TaskList taskList)
        {
            _context.TaskLists.Update(taskList);
            await _context.SaveChangesAsync();
        }
        private DetectionTask MapToAggregate(
            TaskList taskList,
            List<AlgoTask> algoTasks,
            List<Localfile> localFiles,
            List<TaskTypeMap> taskTypeMaps,
            List<AlgoType> algoTypes)
        {
            //  URL（按顺序）
            var urls = localFiles
                .OrderBy(f => f.Sid)
                .Select(f => f.UrlLocal)
                .ToList();

            // 构建 TypeId → AiServiceType 映射
            var typeDict = algoTypes.ToDictionary(
                t => t.Id,
                t => Enum.Parse<AiServiceType>(t.Name, true) // name 和 enum 一致
            );

            // 每个 AlgoTask 对应的 Types
            var algoTaskTypesDict = taskTypeMaps
                .GroupBy(m => m.TaskId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => typeDict[m.TypeId]).ToList()
                );

            // DetectionTask 的 Types（可以取 union 或第一个）
            var detectionTypes = algoTaskTypesDict.Values
                .SelectMany(x => x)
                .Distinct()
                .ToList();

            // 创建 DetectionTask
            var detectionTask = new DetectionTask(taskList.UserId, urls, detectionTypes, taskList.Level);

            // 回填基础字段（重要）
            typeof(DetectionTask).GetProperty("Id")!
                .SetValue(detectionTask, taskList.Id);

            typeof(DetectionTask).GetProperty("CreatedAt")!
                .SetValue(detectionTask, taskList.CreatedAt);

            typeof(DetectionTask).GetProperty("UpdatedAt")!
                .SetValue(detectionTask, taskList.UpdatedAt);

            // 同步状态（通过 AlgoTask）
            foreach (var algo in algoTasks)
            {
                if (algo.Status == (int)AlgoTaskStatus.done)
                {
                    detectionTask.OnAlgoTaskCompleted();
                }
            }

            return detectionTask;
        }
    }
}
