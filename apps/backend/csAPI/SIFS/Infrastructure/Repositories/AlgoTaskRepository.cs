using Microsoft.EntityFrameworkCore;
using SIFS.Domain.Entities;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class AlgoTaskRepository: IAlgoTaskRepository
    {
        private readonly SIFSContext _context;
        public AlgoTaskRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task<Result<AlgoTask>> GetTaskByIdAsync(Guid id)
        {
            var algoTask = await _context.AlgoTasks.FirstOrDefaultAsync(t => t.Id == id);
            return algoTask != null
                ? Result<AlgoTask>.Success(algoTask)
                : Result<AlgoTask>.Fail(ResultCode.NotFound, "算法任务记录不存在");
        }
        public async Task<Result<TaskItem>> GetAggregateByGuidAsync(Guid id)
        {
            // 取 AlgoTask 基础实体
            var entity = await _context.AlgoTasks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                return null;

            // 取 Localfile
            var localFile = await _context.Localfiles
                .FirstOrDefaultAsync(f => f.AlgoTaskId == id);

            if (localFile == null)
                throw new Exception($"AlgoTask {id} 没有对应的 Localfile");

            // 取 TaskTypeMap
            var taskTypeMap = await _context.TaskTypeMaps
            .FirstOrDefaultAsync(m => m.TaskId == id);

            if (taskTypeMap == null)
                throw new Exception($"AlgoTask {id} 没有对应的 TaskTypeMap");

            // 取 AlgoType
            var algoType = await _context.AlgoTypes
            .FirstOrDefaultAsync(t => t.Id == taskTypeMap.TypeId);

            if (algoType == null)
                throw new Exception($"TypeId {taskTypeMap.TypeId} 未找到对应的 AlgoType");

            // Map 聚合
            var taskAggregate = MapToAggregate(entity, localFile, algoType);

            return Result<TaskItem>.Success(taskAggregate);
        }
        public async Task InsertAsync(AlgoTask algoTask)
        {
            await _context.AlgoTasks.AddAsync(algoTask);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(AlgoTask algoTask)
        {
            _context.AlgoTasks.Update(algoTask);
            await _context.SaveChangesAsync();
        }
        private TaskItem MapToAggregate(
            AlgoTask entity,
            Localfile localFile,
            AlgoType algoType)
        {
            // 枚举类型转换
            var type = Enum.Parse<AiServiceType>(algoType.Name, true);

            // 构建 TaskItem
            var task = new TaskItem(entity.TaskId, localFile.UrlLocal, new List<AiServiceType> { type });

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
                    task.MarkAsDone(new List<DetectionResult>()); // 占位
                    break;
                case AlgoTaskStatus.failed:
                    task.MarkAsFailed();
                    break;
            }

            return task;
        }
    }
}
