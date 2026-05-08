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
            var taskResult = await _algoTaskRepo.GetTaskByIdAsync(algoTaskId);
            if (!taskResult.IsSuccess || !taskResult.Data.AlgoModelId.HasValue)
            {
                _logger.LogWarning("算法任务 {AlgoTaskId} 缺少有效算法模型，跳过执行", algoTaskId);
                return;
            }

            if (!await _algoTaskRepo.TryMarkRunningAsync(algoTaskId, taskResult.Data.AlgoModelId.Value))
            {
                _logger.LogInformation("算法任务 {AlgoTaskId} 未抢占成功，跳过执行", algoTaskId);
                return;
            }

            try
            {
                await _taskAuditService.RecordTransitionAsync(
                    taskResult.Data.TaskId,
                    "pending",
                    "processing",
                    "worker started task",
                    null,
                    new { algo_task_id = taskResult.Data.Id, algorithm = taskResult.Data.AlgoName });

                var executionResult = await ExecuteCoreAsync(algoTaskId);
                if (await _algoTaskRepo.TryMarkDoneAsync(algoTaskId))
                    await HandleExecutionSucceededAsync(algoTaskId, executionResult);
            }
            catch (Exception ex)
            {
                var failureReason = ToSafeFailureReason(ex);
                if (await _algoTaskRepo.TryMarkFailedAsync(algoTaskId, failureReason))
                    await HandleExecutionFailedAsync(algoTaskId, failureReason);

                _logger.LogError(ex, "执行算法任务 {AlgoTaskId} 失败", algoTaskId);
            }
        }

        public async Task<AlgoTaskExecutionResult> ExecuteCoreAsync(Guid algoTaskId)
        {
            var taskResult = await _algoTaskRepo.GetAggregateByGuidAsync(algoTaskId);
            if (!taskResult.IsSuccess)
                throw new InvalidOperationException(taskResult.Message);

            var task = taskResult.Data;

            if (task.Status != SIFS.Domain.Enum.AlgoTaskStatus.running)
                throw new InvalidOperationException($"task is not running: {task.Status}");

            var parentEntityResult = await _taskListRepo.GetTaskListByIdAsync(task.TaskId);
            if (!parentEntityResult.IsSuccess)
                throw new InvalidOperationException("parent task missing");

            var parentEntity = parentEntityResult.Data;
            var accessibleUrl = _urlBuilder.ToAbsoluteUrl(task.Url);
            var result = await _aiService
                .DetectAsync(accessibleUrl, task.Level, task.AlgoApiUrl ?? string.Empty, task.AlgoName, parentEntity.UserId);

            _logger.LogInformation("算法任务 {AlgoTaskId} 执行完成，结果: {IsFake},{Confidence},{Url}", algoTaskId, result.IsFake, result.Confidence, result.MaskUrl);

            if (!await _algoTaskRepo.IsRunningAsync(algoTaskId))
                throw new InvalidOperationException("task is no longer running");

            var resultFile = new ResultFile
            {
                Id = UuidV7.NewUuidV7(),
                AlgoTaskId = task.Id,
                IsFake = result.IsFake,
                Confidence = result.Confidence,
                MaskLocalUrl = result.MaskUrl,
            };

            await _resultFileRepository.InsertAsync(resultFile);
            _logger.LogInformation("保存算法任务 {AlgoTaskId} 的结果文件记录，MaskLocalUrl: {MaskLocalUrl}", algoTaskId, result.MaskUrl);

            return new AlgoTaskExecutionResult
            {
                TaskId = task.TaskId,
                AlgoTaskId = task.Id,
                UserId = parentEntity.UserId,
                Algorithm = task.AlgoName,
                ResultUrl = result.MaskUrl
            };
        }

        public async Task HandleExecutionSucceededAsync(Guid algoTaskId, AlgoTaskExecutionResult executionResult)
        {
            await _taskAuditService.RecordTransitionAsync(
                executionResult.TaskId,
                "processing",
                "success",
                "worker completed task",
                null,
                new { algo_task_id = algoTaskId, algorithm = executionResult.Algorithm });

            var progress = await _taskListRepo.RefreshProgressFromSubTasksAsync(executionResult.TaskId);

            await _taskNotificationService.NotifyAlgoTaskFinishedAsync(new TaskFinishedNotification
            {
                UserId = executionResult.UserId,
                TaskId = executionResult.TaskId,
                AlgoTaskId = executionResult.AlgoTaskId,
                Status = "done",
                StatusText = "已完成",
                Algorithm = executionResult.Algorithm,
                ResultUrl = executionResult.ResultUrl,
                ParentTaskCompleted = progress.IsCompleted,
                FinishedAt = DateTime.UtcNow
            });
        }

        public async Task HandleExecutionFailedAsync(Guid algoTaskId, string failureReason)
        {
            var taskResult = await _algoTaskRepo.GetAggregateByGuidAsync(algoTaskId);
            if (!taskResult.IsSuccess)
                return;

            var task = taskResult.Data;
            var fromStatus = task.StartedAt.HasValue ? "processing" : "pending";
            await _taskAuditService.RecordTransitionAsync(
                task.TaskId,
                fromStatus,
                "failed",
                failureReason,
                null,
                new { algo_task_id = task.Id, algorithm = task.AlgoName });

            await _taskListRepo.RefreshProgressFromSubTasksAsync(task.TaskId);

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
                    FailureReason = failureReason,
                    ParentTaskCompleted = false,
                    FinishedAt = DateTime.UtcNow
                });
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
