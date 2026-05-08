using SIFS.Application.TaskAudits;
using SIFS.Domain.Enum;
using SIFS.Infrastructure;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Results;

namespace SIFS.Application.TaskManagement
{
    public class TaskManagementService : ITaskManagementService
    {
        private readonly ITaskManagementRepository _taskManagementRepository;
        private readonly IAlgorithmEndpointResolver _algorithmEndpointResolver;
        private readonly IEventBus _eventBus;
        private readonly IAppEventRequestContextFactory _requestContextFactory;
        private readonly IAlgoTaskQueue _queue;
        private readonly ITaskAuditService _taskAuditService;

        public TaskManagementService(
            ITaskManagementRepository taskManagementRepository,
            IAlgorithmEndpointResolver algorithmEndpointResolver,
            IEventBus eventBus,
            IAppEventRequestContextFactory requestContextFactory,
            IAlgoTaskQueue queue,
            ITaskAuditService taskAuditService)
        {
            _taskManagementRepository = taskManagementRepository;
            _algorithmEndpointResolver = algorithmEndpointResolver;
            _eventBus = eventBus;
            _requestContextFactory = requestContextFactory;
            _queue = queue;
            _taskAuditService = taskAuditService;
        }

        public async Task<Result<Paged<TaskManagementListItemDto>>> QueryAdminAsync(TaskManagementQuery query, Guid actorId)
        {
            var page = await _taskManagementRepository.QueryAsync(query);
            return Result<Paged<TaskManagementListItemDto>>.Success(page);
        }

        public async Task<Result<Paged<TaskManagementListItemDto>>> QueryUserAsync(TaskManagementQuery query, Guid userId)
        {
            var page = await _taskManagementRepository.QueryAsync(query, userId);
            return Result<Paged<TaskManagementListItemDto>>.Success(page);
        }

        public async Task<Result<TaskManagementDetailDto>> GetAdminDetailAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskManagementDetailDto>.Fail(ResultCode.NotFound, "Task not found");

            detail.StatusTimeline = await _taskAuditService.ListByTaskIdAsync(taskId);
            PublishTaskViewed(taskId, actorId, "admin view task");
            return Result<TaskManagementDetailDto>.Success(detail);
        }

        public async Task<Result<TaskManagementDetailDto>> GetUserDetailAsync(Guid taskId, Guid userId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId, userId);
            if (detail == null)
                return Result<TaskManagementDetailDto>.Fail(ResultCode.Forbidden, "No permission to access this task");

            detail.StatusTimeline = await _taskAuditService.ListByTaskIdAsync(taskId);
            PublishTaskViewed(taskId, userId, "view own task");
            return Result<TaskManagementDetailDto>.Success(detail);
        }

        public async Task<Result<List<TaskStatusFlowItemDto>>> GetAdminStatusFlowAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId, includeDeleted: true);
            if (detail == null)
                return Result<List<TaskStatusFlowItemDto>>.Fail(ResultCode.NotFound, "Task not found");

            var flow = (await _taskAuditService.ListByTaskIdAsync(taskId))
                .Select(x => new TaskStatusFlowItemDto
                {
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Status = x.ToStatus,
                    Reason = x.Reason,
                    OperatorId = x.OperatorId,
                    CreatedAt = x.CreatedAt,
                    ExtraJson = x.ExtraJson
                })
                .ToList();

            PublishTaskViewed(taskId, actorId, "view task status flow");
            return Result<List<TaskStatusFlowItemDto>>.Success(flow);
        }

        public async Task<Result<TaskOperationResultDto>> CancelAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "Task not found");
            if (detail.CurrentStatus is "done" or "deleted" or "canceled")
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "Task status does not allow cancel");

            var fromStatus = detail.CurrentStatus;
            await _taskManagementRepository.CancelAsync(taskId, "canceled by admin");
            await _taskAuditService.RecordTransitionAsync(taskId, fromStatus, "canceled", "admin canceled task", actorId);
            PublishTaskDeleted(taskId, actorId, "cancel task", "cancel");

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                Message = "Task canceled"
            });
        }

        public async Task<Result<TaskOperationResultDto>> RetryAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "Task not found");

            var retryTargets = detail.SubTasks
                .Where(x => x.AlgorithmId.HasValue || !string.IsNullOrWhiteSpace(x.AlgorithmName))
                .ToList();

            if (!retryTargets.Any())
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "Task has no algorithm info for retry");

            var endpoints = new Dictionary<Guid, AlgorithmEndpointResolution>();
            foreach (var subTask in retryTargets)
            {
                var resolveResult = await _algorithmEndpointResolver.ResolveAsync(subTask.AlgorithmId, subTask.AlgorithmName);
                if (!resolveResult.IsSuccess)
                    return Result<TaskOperationResultDto>.Fail(resolveResult.Code, resolveResult.Message);

                endpoints[subTask.TaskId] = resolveResult.Data;
            }

            var retryResult = await _taskManagementRepository.RetryAsync(taskId, endpoints);
            await _taskAuditService.RecordTransitionAsync(taskId, detail.CurrentStatus, "retried", "admin retried task", actorId, new { new_task_id = retryResult.NewTaskId });
            await _taskAuditService.RecordTransitionAsync(retryResult.NewTaskId, null, "created", "created from retry", actorId, new { source_task_id = taskId });

            foreach (var algoTask in retryResult.AlgoTasks)
            {
                await _queue.EnqueueAsync(algoTask);
                await _taskAuditService.RecordTransitionAsync(retryResult.NewTaskId, "created", "queued", "task queued", actorId, new { algo_task_id = algoTask.TaskId });
            }

            _eventBus.Publish(new AppEvent
            {
                EventType = AppEventTypes.TaskRetried,
                ActorId = actorId,
                TargetType = "detection_task",
                TargetId = taskId.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["new_task_id"] = retryResult.NewTaskId,
                    ["sub_task_count"] = retryResult.AlgoTasks.Count
                },
                RequestContext = _requestContextFactory.Create("retry task")
            });

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                NewTaskId = retryResult.NewTaskId,
                Message = "Task retried"
            });
        }

        public async Task<Result<TaskOperationResultDto>> DeleteAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "Task not found");
            if (detail.CurrentStatus == "deleted")
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "Task already deleted");

            var fromStatus = detail.CurrentStatus;
            await _taskManagementRepository.SoftDeleteAsync(taskId, "deleted by admin");
            await _taskAuditService.RecordTransitionAsync(taskId, fromStatus, "deleted", "admin deleted task", actorId);
            PublishTaskDeleted(taskId, actorId, "delete task", "delete");

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                Message = "Task deleted"
            });
        }

        private void PublishTaskViewed(Guid taskId, Guid actorId, string summary)
        {
            _eventBus.Publish(new AppEvent
            {
                EventType = AppEventTypes.TaskViewed,
                ActorId = actorId,
                TargetType = "detection_task",
                TargetId = taskId.ToString(),
                RequestContext = _requestContextFactory.Create(summary)
            });
        }

        private void PublishTaskDeleted(Guid taskId, Guid actorId, string summary, string action)
        {
            _eventBus.Publish(new AppEvent
            {
                EventType = AppEventTypes.TaskDeleted,
                ActorId = actorId,
                TargetType = "detection_task",
                TargetId = taskId.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["action"] = action
                },
                RequestContext = _requestContextFactory.Create(summary)
            });
        }
    }
}
