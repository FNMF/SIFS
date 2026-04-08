using SIFS.Application.DetectionApp;
using SIFS.Infrastructure.Repositories;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoTaskAppService: IAlgoTaskAppService
    {
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ITaskListRepository _taskListRepo;
        private readonly IDetectionService _detectionService;

        public AlgoTaskAppService(
            IAlgoTaskRepository algoTaskRepo,
            ITaskListRepository taskListRepo,
            IDetectionService detectionService)
        {
            _algoTaskRepo = algoTaskRepo;
            _taskListRepo = taskListRepo;
            _detectionService = detectionService;
        }

        public async Task ExecuteAsync(Guid algoTaskId)
        {
            // 获取完整聚合
            var taskResult = await _algoTaskRepo.GetAggregateByGuidAsync(algoTaskId);
            if (!taskResult.IsSuccess)
            {
                // 记录日志
                return;
            }
            var task = taskResult.Data;

            try
            {
                // 标记运行中
                task.MarkAsRunning();
                await _algoTaskRepo.UpdateAsync(task.ToEntity());

                // 调用 AI
                var results = await _detectionService
                    .DetectSelected(task.Url, task.Types);

                // 标记完成
                task.MarkAsDone(results);
                await _algoTaskRepo.UpdateAsync(task.ToEntity());

                // 更新父任务
                var parentEntityResult = await _taskListRepo.GetTaskListByIdAsync(task.TaskId);

                var parentEntity = parentEntityResult.Data;
                parentEntity.Status += 1;
                parentEntity.UpdatedAt = DateTime.UtcNow;

                await _taskListRepo.UpdateAsync(parentEntity);

                // 判断是否完成（推荐直接查）
                var detectionTaskResult = await _taskListRepo.GetDetectionTaskAggregateByGuidAsync(task.TaskId);
                var detectionTask = detectionTaskResult.Data;


                if (detectionTask.IsCompleted)
                {
                    // SignalR
                    // await _hub.NotifyCompleted(task.TaskId);
                }
            }
            catch (Exception ex)
            {
                // 失败处理
                task.MarkAsFailed();
                await _algoTaskRepo.UpdateAsync(task.ToEntity());

                // TODO: 日志 / 重试
            }
        }
    }
}
