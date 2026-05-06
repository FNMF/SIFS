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

        public TaskManagementService(
            ITaskManagementRepository taskManagementRepository,
            IAlgorithmEndpointResolver algorithmEndpointResolver,
            IEventBus eventBus,
            IAppEventRequestContextFactory requestContextFactory,
            IAlgoTaskQueue queue)
        {
            _taskManagementRepository = taskManagementRepository;
            _algorithmEndpointResolver = algorithmEndpointResolver;
            _eventBus = eventBus;
            _requestContextFactory = requestContextFactory;
            _queue = queue;
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
                return Result<TaskManagementDetailDto>.Fail(ResultCode.NotFound, "任务不存在");

            PublishTaskViewed(taskId, actorId, "admin view task");
            return Result<TaskManagementDetailDto>.Success(detail);
        }

        public async Task<Result<TaskManagementDetailDto>> GetUserDetailAsync(Guid taskId, Guid userId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId, userId);
            if (detail == null)
                return Result<TaskManagementDetailDto>.Fail(ResultCode.Forbidden, "无权访问该任务");

            PublishTaskViewed(taskId, userId, "view own task");
            return Result<TaskManagementDetailDto>.Success(detail);
        }

        public async Task<Result<List<TaskStatusFlowItemDto>>> GetAdminStatusFlowAsync(Guid taskId, Guid actorId)
        {
            var flow = await _taskManagementRepository.GetStatusFlowAsync(taskId);
            if (flow == null)
                return Result<List<TaskStatusFlowItemDto>>.Fail(ResultCode.NotFound, "任务不存在");

            PublishTaskViewed(taskId, actorId, "view task status flow");
            return Result<List<TaskStatusFlowItemDto>>.Success(flow);
        }

        public async Task<Result<TaskOperationResultDto>> CancelAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "任务不存在");
            if (detail.CurrentStatus is "done" or "deleted" or "canceled")
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "当前任务状态不允许取消");

            await _taskManagementRepository.CancelAsync(taskId, "canceled by admin");
            PublishTaskDeleted(taskId, actorId, "cancel task", "cancel");

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                Message = "任务已取消"
            });
        }

        public async Task<Result<TaskOperationResultDto>> RetryAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "任务不存在");

            var typeIds = detail.SubTasks
                .Select(x => x.AlgorithmTypeId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (!typeIds.Any())
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "任务缺少算法信息，无法重试");

            var endpoints = new Dictionary<int, AlgorithmEndpointResolution>();
            foreach (var typeId in typeIds)
            {
                if (!Enum.IsDefined(typeof(AiServiceType), typeId))
                    return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, $"ALGORITHM_NOT_FOUND: {typeId}");

                var resolveResult = await _algorithmEndpointResolver.ResolveAsync((AiServiceType)typeId);
                if (!resolveResult.IsSuccess)
                    return Result<TaskOperationResultDto>.Fail(resolveResult.Code, resolveResult.Message);

                endpoints[typeId] = resolveResult.Data;
            }

            var retryResult = await _taskManagementRepository.RetryAsync(taskId, endpoints);
            foreach (var algoTaskId in retryResult.AlgoTaskIds)
            {
                await _queue.EnqueueAsync(algoTaskId);
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
                    ["sub_task_count"] = retryResult.AlgoTaskIds.Count
                },
                RequestContext = _requestContextFactory.Create("retry task")
            });

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                NewTaskId = retryResult.NewTaskId,
                Message = "任务已重新提交"
            });
        }

        public async Task<Result<TaskOperationResultDto>> DeleteAdminAsync(Guid taskId, Guid actorId)
        {
            var detail = await _taskManagementRepository.GetDetailAsync(taskId);
            if (detail == null)
                return Result<TaskOperationResultDto>.Fail(ResultCode.NotFound, "任务不存在");
            if (detail.CurrentStatus == "deleted")
                return Result<TaskOperationResultDto>.Fail(ResultCode.InvalidInput, "任务已删除");

            await _taskManagementRepository.SoftDeleteAsync(taskId, "deleted by admin");
            PublishTaskDeleted(taskId, actorId, "delete task", "delete");

            return Result<TaskOperationResultDto>.Success(new TaskOperationResultDto
            {
                TaskId = taskId,
                Message = "任务已删除"
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
