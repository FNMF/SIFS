using SIFS.Api.DetectionTask;
using SIFS.Infrastructure.Repositories;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Domain.Entities;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;
using Microsoft.AspNetCore.Mvc;
using SIFS.Domain.Enum;
using System.Diagnostics;
using SIFS.Infrastructure.External;
using SIFS.Application.Rbac;
using SIFS.Application.TaskAudits;
using SIFS.Shared.Extensions.EventBus;

namespace SIFS.Application.DetectionTaskApp
{
    public class DetectionTaskAppService : IDetectionTaskAppService
    {
        private readonly ILocalfileService _localfileService;
        private readonly ITaskListRepository _taskListRepo;
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ILocalfileRepository _fileRepo;
        private readonly ITaskTypeMapRepository _taskTypeMapRepo;
        private readonly IAlgoTaskQueue _queue;
        private readonly IPermissionService _permissionService;
        private readonly IEventBus _eventBus;
        private readonly IAppEventRequestContextFactory _requestContextFactory;
        private readonly IAlgorithmEndpointResolver _algorithmEndpointResolver;
        private readonly ITaskAuditService _taskAuditService;
        private readonly ILogger<DetectionTaskAppService> _logger;

        public DetectionTaskAppService(
            ILocalfileService localfileService,
            ITaskListRepository taskListRepo,
            IAlgoTaskRepository algoTaskRepo,
            ILocalfileRepository fileRepo,
            ITaskTypeMapRepository taskTypeMapRepo,
            IAlgoTaskQueue queue,
            IPermissionService permissionService,
            IEventBus eventBus,
            IAppEventRequestContextFactory requestContextFactory,
            IAlgorithmEndpointResolver algorithmEndpointResolver,
            ITaskAuditService taskAuditService,
            ILogger<DetectionTaskAppService> logger)
        {
            _localfileService = localfileService;
            _taskListRepo = taskListRepo;
            _algoTaskRepo = algoTaskRepo;
            _fileRepo = fileRepo;
            _taskTypeMapRepo = taskTypeMapRepo;
            _queue = queue;
            _permissionService = permissionService;
            _eventBus = eventBus;
            _requestContextFactory = requestContextFactory;
            _algorithmEndpointResolver = algorithmEndpointResolver;
            _taskAuditService = taskAuditService;
            _logger = logger;
        }

        public async Task<Result<Guid>> CreateAsync(CreateDetectionTaskDto dto, Guid userId)
        {
            try
            {
                // 基础校验
                if (dto.Images == null || !dto.Images.Any())
                    throw new Exception("请至少上传一张图片");
                if (dto.Types == null || !dto.Types.Any())
                    throw new Exception("请至少选择一个检测类型");

                var algorithmEndpoints = new Dictionary<AiServiceType, AlgorithmEndpointResolution>();
                foreach (var type in dto.Types.Distinct())
                {
                    var resolveResult = await _algorithmEndpointResolver.ResolveAsync(type);
                    if (!resolveResult.IsSuccess)
                        return Result<Guid>.Fail(resolveResult.Code, resolveResult.Message);

                    algorithmEndpoints[type] = resolveResult.Data;
                }

                // 排序校验（防重复）
                if (dto.Images.Select(x => x.Order).Distinct().Count() != dto.Images.Count)
                    throw new Exception("图片排序重复");

                // 排序规范化
                var normalizedImages = dto.Images
                    .OrderBy(x => x.Order)
                    .Select((x, index) => new { x.File, Order = index })
                    .ToList();

                // 保存文件
                var urls = new List<string>();
                var fileResults = new List<(string Url, int Order)>();
                foreach (var img in normalizedImages)
                {
                    var url = await _localfileService.LocalSaveAsync(img.File, userId);
                    urls.Add(url);
                    fileResults.Add((url, img.Order));
                }

                // 创建DetectionTask的Domain对象并持久化
                var detectionTask = new DetectionTask(userId, urls, dto.Types, dto.Level);

                await _taskListRepo.InsertAsync(detectionTask.ToEntity());
                await _taskAuditService.RecordTransitionAsync(
                    detectionTask.Id,
                    null,
                    "created",
                    "task created",
                    userId);

                // 生成并持久化子任务
                foreach (var file in fileResults)
                {
                    foreach (var type in dto.Types)
                    {
                        // 创建 AlgoTask
                        var taskItem = new TaskItem(detectionTask.Id, file.Url, type, dto.Level);
                        var endpoint = algorithmEndpoints[type];
                        taskItem.SetAlgorithmEndpoint(endpoint.AlgoModelId, endpoint.AlgoName, endpoint.ApiUrl);
                        var algoTask = taskItem.ToEntity();

                        await _algoTaskRepo.InsertAsync(algoTask);

                        // Localfile（一个 task 对应一个文件）
                        var localFile = new Localfile
                        {
                            Id = UuidV7.NewUuidV7(),
                            UrlLocal = file.Url,
                            AlgoTaskId = algoTask.Id,
                            Sid = file.Order,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _fileRepo.CreateLocalfileAsync(localFile);

                        // TaskTypeMap
                        var typeMap = new TaskTypeMap
                        {
                            Id = UuidV7.NewUuidV7(),
                            TaskId = algoTask.Id,   // 这里是 AlgoTaskId
                            TypeId = (int)type     // 枚举值和数据库一致
                        };

                        await _taskTypeMapRepo.InsertAsync(typeMap);

                        // 入队
                        await _queue.EnqueueAsync(algoTask.Id);
                        await _taskAuditService.RecordTransitionAsync(
                            detectionTask.Id,
                            "created",
                            "queued",
                            "task queued",
                            userId,
                            new { algo_task_id = algoTask.Id, algorithm = endpoint.AlgoName });
                    }
                }
                // 返回任务ID
                _eventBus.Publish(new AppEvent
                {
                    EventType = AppEventTypes.TaskCreated,
                    ActorId = userId,
                    TargetType = "detection_task",
                    TargetId = detectionTask.Id.ToString(),
                    Payload = new Dictionary<string, object?>
                    {
                        ["image_count"] = normalizedImages.Count,
                        ["type_count"] = dto.Types.Count,
                        ["level"] = dto.Level
                    },
                    RequestContext = _requestContextFactory.Create("create detection task")
                });
                return Result<Guid>.Success(detectionTask.Id);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }
        public async Task<Result<List<DetectionTaskReadDto>>> GetAllAsync(Guid userId)
        {
            try
            {
                var canViewAll = await CanViewAllTasksAsync(userId);
                var data = canViewAll
                    ? await _taskListRepo.GetAllReadDtosAsync()
                    : await _taskListRepo.GetAllReadDtosByUserIdAsync(userId);

                return Result<List<DetectionTaskReadDto>>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<List<DetectionTaskReadDto>>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }
        public async Task<Result<DetectionTaskDetailDto>> GetAsync(Guid guid, Guid userId)
        {
            try
            {
                var taskListResult = await _taskListRepo.GetTaskListByIdAsync(guid);
                if (!taskListResult.IsSuccess)
                    return Result<DetectionTaskDetailDto>.Fail(taskListResult.Code, taskListResult.Message);

                if (taskListResult.Data.UserId != userId && !await CanViewAllTasksAsync(userId))
                    return Result<DetectionTaskDetailDto>.Fail(ResultCode.Forbidden, "无权访问该任务");

                var task = taskListResult.Data;

                var algoTasks = await _algoTaskRepo.GetAllReadDtosByTaskIdAsync(guid);

                var imageUrls = await _taskListRepo.GetImageUrlsByTaskIdAsync(guid);

                var dto = new DetectionTaskDetailDto
                {
                    Guid = task.Id,
                    ImageUrls = imageUrls,
                    PreviewImageUrl = imageUrls.FirstOrDefault() ?? string.Empty,
                    ImageCount = imageUrls.Count,
                    SubTaskCount = algoTasks.Count,
                    CompletedSubTaskCount = algoTasks.Count(x => x.Status == (int)AlgoTaskStatus.done),
                    Completion = algoTasks.Count == 0
                        ? 0m
                        : Math.Round((decimal)algoTasks.Count(x => x.Status == (int)AlgoTaskStatus.done) / algoTasks.Count, 4),
                    Level = task.Level,
                    UpdatedAt = task.UpdatedAt,
                    AlgoTasks = algoTasks
                };

                _eventBus.Publish(new AppEvent
                {
                    EventType = AppEventTypes.TaskViewed,
                    ActorId = userId,
                    TargetType = "detection_task",
                    TargetId = task.Id.ToString(),
                    RequestContext = _requestContextFactory.Create("view detection task")
                });

                return Result<DetectionTaskDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<DetectionTaskDetailDto>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }

        private async Task<bool> CanViewAllTasksAsync(Guid userId)
        {
            var result = await _permissionService.HasPermissionAsync(userId, "task:view:all");
            return result.IsSuccess && result.Data;
        }
    }
}

