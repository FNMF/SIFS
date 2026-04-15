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
        private readonly ILogger<DetectionTaskAppService> _logger;

        public DetectionTaskAppService(
            ILocalfileService localfileService,
            ITaskListRepository taskListRepo,
            IAlgoTaskRepository algoTaskRepo,
            ILocalfileRepository fileRepo,
            ITaskTypeMapRepository taskTypeMapRepo,
            IAlgoTaskQueue queue,
            ILogger<DetectionTaskAppService> logger)
        {
            _localfileService = localfileService;
            _taskListRepo = taskListRepo;
            _algoTaskRepo = algoTaskRepo;
            _fileRepo = fileRepo;
            _taskTypeMapRepo = taskTypeMapRepo;
            _queue = queue;
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
                    var url = await _localfileService.LocalSaveAsync(img.File);
                    urls.Add(url);
                    fileResults.Add((url, img.Order));
                }

                // 创建DetectionTask的Domain对象并持久化
                var detectionTask = new DetectionTask(userId, urls, dto.Types, dto.Level);

                await _taskListRepo.InsertAsync(detectionTask.ToEntity());

                // 生成并持久化子任务
                foreach (var file in fileResults)
                {
                    foreach (var type in dto.Types)
                    {
                        // 创建 AlgoTask
                        var taskItem = new TaskItem(detectionTask.Id, file.Url, type, dto.Level);
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
                    }
                }
                // 返回任务ID
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
                var data = await _taskListRepo.GetAllReadDtosByUserIdAsync(userId);
                return Result<List<DetectionTaskReadDto>>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<List<DetectionTaskReadDto>>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }
        public async Task<Result<List<AlgoReadDto>>> GetAsync(Guid guid, Guid userId)
        {
            try
            {
                var taskListResult = await _taskListRepo.GetTaskListByIdAsync(guid);
                if (!taskListResult.IsSuccess)
                    return Result<List<AlgoReadDto>>.Fail(taskListResult.Code, taskListResult.Message);

                if (taskListResult.Data.UserId != userId)
                    return Result<List<AlgoReadDto>>.Fail(ResultCode.Forbidden, "无权访问该任务");

                var data = await _algoTaskRepo.GetAllReadDtosByTaskIdAsync(guid);
                return Result<List<AlgoReadDto>>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<List<AlgoReadDto>>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }
    }
}

