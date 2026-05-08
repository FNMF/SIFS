using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;
using SIFS.Application.Rbac;
using SIFS.Application.TaskAudits;
using SIFS.Infrastructure.Realtime;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoTaskAppService: IAlgoTaskAppService
    {
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ITaskListRepository _taskListRepo;
        private readonly IResultFileRepository _resultFileRepository;
        private readonly IAiService _aiService;
        private readonly IFileUrlBuilder _urlBuilder;
        private readonly IPermissionService _permissionService;
        private readonly ITaskAuditService _taskAuditService;
        private readonly ITaskNotificationService _taskNotificationService;
        private readonly ILogger<AlgoTaskAppService> _logger;

        public AlgoTaskAppService(
            IAlgoTaskRepository algoTaskRepo,
            ITaskListRepository taskListRepo,
            IResultFileRepository resultFileRepository,
            IAiService aiService,
            IFileUrlBuilder urlBuilder,
            IPermissionService permissionService,
            ITaskAuditService taskAuditService,
            ITaskNotificationService taskNotificationService,
            ILogger<AlgoTaskAppService> logger)
        {
            _algoTaskRepo = algoTaskRepo;
            _taskListRepo = taskListRepo;
            _resultFileRepository = resultFileRepository;
            _aiService = aiService;
            _urlBuilder = urlBuilder;
            _permissionService = permissionService;
            _taskAuditService = taskAuditService;
            _taskNotificationService = taskNotificationService;
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
            if (task.Status != SIFS.Domain.Enum.AlgoTaskStatus.pending)
            {
                _logger.LogInformation("算法任务 {AlgoTaskId} 当前状态为 {Status}，跳过执行", algoTaskId, task.Status);
                return;
            }

            try
            {
                _logger.LogInformation("开始执行算法任务 {AlgoTaskId}", algoTaskId);
                // 标记运行中
                var fromStatus = task.Status.ToString();
                task.MarkAsRunning();
                await _algoTaskRepo.UpdateAsync(task.ToEntity());
                await _taskAuditService.RecordTransitionAsync(
                    task.TaskId,
                    fromStatus,
                    "processing",
                    "worker started task",
                    null,
                    new { algo_task_id = task.Id, algorithm = task.AlgoName });

                var parentEntityResult = await _taskListRepo.GetTaskListByIdAsync(task.TaskId);
                if (!parentEntityResult.IsSuccess)
                    throw new InvalidOperationException("parent task missing");

                var parentEntity = parentEntityResult.Data;

                // URL 转换（本地路径 -> 可访问 URL）
                var accessibleUrl = _urlBuilder.ToAbsoluteUrl(task.Url);
                // 调用 AI
                var result = await _aiService
                    .DetectAsync(accessibleUrl, task.Level, task.AlgoApiUrl ?? string.Empty, task.AlgoName, parentEntity.UserId);
                _logger.LogInformation("算法任务 {AlgoTaskId} 执行完成，结果: {IsFake},{Confidence},{Url}", algoTaskId, result.IsFake,result.Confidence,result.MaskUrl);

                // 保存结果文件记录
                var resultFile = new ResultFile
                {
                    Id = UuidV7.NewUuidV7(),
                    AlgoTaskId = task.Id,
                    AlgoType = task.AlgoModelId ?? 0,
                    IsFake = result.IsFake,
                    Confidence = result.Confidence,
                    MaskLocalUrl = result.MaskUrl,
                };
                
                await _resultFileRepository.InsertAsync(resultFile);
                _logger.LogInformation("保存算法任务 {AlgoTaskId} 的结果文件记录，MaskLocalUrl: {MaskLocalUrl}", algoTaskId, result.MaskUrl);

                // 标记完成
                fromStatus = task.Status.ToString();
                task.MarkAsDone(result);
                await _algoTaskRepo.UpdateAsync(task.ToEntity());
                await _taskAuditService.RecordTransitionAsync(
                    task.TaskId,
                    fromStatus,
                    "success",
                    "worker completed task",
                    null,
                    new { algo_task_id = task.Id, algorithm = task.AlgoName });

                // 更新父任务
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

                await _taskNotificationService.NotifyAlgoTaskFinishedAsync(new TaskFinishedNotification
                {
                    UserId = parentEntity.UserId,
                    TaskId = task.TaskId,
                    AlgoTaskId = task.Id,
                    Status = "done",
                    StatusText = "已完成",
                    Algorithm = task.AlgoName,
                    ResultUrl = result.MaskUrl,
                    ParentTaskCompleted = detectionTask.IsCompleted,
                    FinishedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // 失败处理
                var fromStatus = task.Status.ToString();
                task.MarkAsFailed(ToSafeFailureReason(ex));
                await _algoTaskRepo.UpdateAsync(task.ToEntity());
                await _taskAuditService.RecordTransitionAsync(
                    task.TaskId,
                    fromStatus,
                    "failed",
                    task.FailureReason,
                    null,
                    new { algo_task_id = task.Id, algorithm = task.AlgoName });

                var parentEntityResult = await _taskListRepo.GetTaskListByIdAsync(task.TaskId);
                if (parentEntityResult.IsSuccess)
                {
                    await _taskNotificationService.NotifyAlgoTaskFinishedAsync(new TaskFinishedNotification
                    {
                        UserId = parentEntityResult.Data.UserId,
                        TaskId = task.TaskId,
                        AlgoTaskId = task.Id,
                        Status = "failed",
                        StatusText = "失败",
                        Algorithm = task.AlgoName,
                        FailureReason = task.FailureReason,
                        ParentTaskCompleted = false,
                        FinishedAt = DateTime.UtcNow
                    });
                }

                // TODO: 日志 / 重试
                _logger.LogError(ex, "执行算法任务 {AlgoTaskId} 失败", algoTaskId);
            }
        }
        public async Task<Result<AlgoTaskDetailDto>> GetDetailAsync(Guid algoTaskId, Guid userId)
        {
            try
            {
                var canViewAll = await CanViewAllTasksAsync(userId);
                var dto = await _algoTaskRepo.GetDetailDtoByIdAsync(algoTaskId, userId, canViewAll);

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

        private async Task<bool> CanViewAllTasksAsync(Guid userId)
        {
            var result = await _permissionService.HasPermissionAsync(userId, "task:view:all");
            return result.IsSuccess && result.Data;
        }

        private static string ToSafeFailureReason(Exception ex)
        {
            return ex switch
            {
                TaskCanceledException => "algorithm request timeout",
                HttpRequestException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm request failed"
                    : ex.Message,
                InvalidOperationException => string.IsNullOrWhiteSpace(ex.Message)
                    ? "algorithm invocation failed"
                    : ex.Message,
                _ => "algorithm invocation failed"
            };
        }
    }
}
