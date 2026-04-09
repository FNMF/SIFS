using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoTaskAppService: IAlgoTaskAppService
    {
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ITaskListRepository _taskListRepo;
        private readonly IResultFileRepository _resultFileRepository;
        private readonly IAiService _aiService;

        public AlgoTaskAppService(
            IAlgoTaskRepository algoTaskRepo,
            ITaskListRepository taskListRepo,
            IResultFileRepository resultFileRepository,
            IAiService aiService)
        {
            _algoTaskRepo = algoTaskRepo;
            _taskListRepo = taskListRepo;
            _resultFileRepository = resultFileRepository;
            _aiService = aiService;
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
                var result = await _aiService
                    .DetectAsync(task.Type, task.Url);

                // 保存结果文件记录
                var resultFile = new ResultFile
                {
                    Id = UuidV7.NewUuidV7(),
                    AlgoTaskId = task.Id,
                    AlgoType = (int)task.Type,
                    IsFake = result.IsFake,
                    Confidence = result.Confidence,
                    MaskLocalUrl = result.MaskUrl,
                };
                await _resultFileRepository.InsertAsync(resultFile);

                // 标记完成
                task.MarkAsDone(result);
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
