using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoTaskAppService: IAlgoTaskAppService
    {
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ITaskListRepository _taskListRepo;
        private readonly IResultFileRepository _resultFileRepository;
        private readonly IAiService _aiService;
        private readonly IFileUrlBuilder _urlBuilder;
        private readonly ILogger<AlgoTaskAppService> _logger;

        public AlgoTaskAppService(
            IAlgoTaskRepository algoTaskRepo,
            ITaskListRepository taskListRepo,
            IResultFileRepository resultFileRepository,
            IAiService aiService,
            IFileUrlBuilder urlBuilder,
            ILogger<AlgoTaskAppService> logger)
        {
            _algoTaskRepo = algoTaskRepo;
            _taskListRepo = taskListRepo;
            _resultFileRepository = resultFileRepository;
            _aiService = aiService;
            _urlBuilder = urlBuilder;
            _logger = logger;
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
                _logger.LogInformation("开始执行算法任务 {AlgoTaskId}", algoTaskId);
                // 标记运行中
                task.MarkAsRunning();
                await _algoTaskRepo.UpdateAsync(task.ToEntity());

                // URL 转换（本地路径 -> 可访问 URL）
                var accessibleUrl = _urlBuilder.ToAbsoluteUrl(task.Url);
                // 调用 AI
                var result = await _aiService
                    .DetectAsync(task.Type, accessibleUrl, task.Level);
                _logger.LogInformation("算法任务 {AlgoTaskId} 执行完成，结果: {IsFake},{Confidence},{Url}", algoTaskId, result.IsFake,result.Confidence,result.MaskUrl);

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
                _logger.LogInformation("保存算法任务 {AlgoTaskId} 的结果文件记录，MaskLocalUrl: {MaskLocalUrl}", algoTaskId, result.MaskUrl);

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
                _logger.LogError(ex, "执行算法任务 {AlgoTaskId} 失败", algoTaskId);
            }
        }
        public async Task<Result<AlgoTaskDetailDto>> GetDetailAsync(Guid algoTaskId, Guid userId)
        {
            try
            {
                var dto = await _algoTaskRepo.GetDetailDtoByIdAsync(algoTaskId, userId);

                if (dto == null)
                {
                    return Result<AlgoTaskDetailDto>.Fail(
                        ResultCode.NotFound,
                        "未找到对应的子任务，或无权访问该任务"
                    );
                }

                return Result<AlgoTaskDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<AlgoTaskDetailDto>.Fail(
                    ResultCode.BusinessError,
                    ex.Message
                );
            }
        }
    }
}
