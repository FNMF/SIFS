using SIFS.Api.DetectionTask;
using SIFS.Infrastructure.Repositories;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Domain.Entities;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.DetectionTaskApp
{
    public class DetectionTaskAppService : IDetectionTaskAppService
    {
        private readonly ILocalfileService _localfileService;
        private readonly ITaskListRepository _detectionTaskRepo;
        private readonly IAlgoTaskRepository _algoTaskRepo;
        private readonly ILocalfileRepository _fileRepo;
        private readonly IAlgoTaskQueue _queue;

        public DetectionTaskAppService(
            ILocalfileService localfileService,
            ITaskListRepository detectionTaskRepo,
            IAlgoTaskRepository algoTaskRepo,
            ILocalfileRepository fileRepo,
            IAlgoTaskQueue queue)
        {
            _localfileService = localfileService;
            _detectionTaskRepo = detectionTaskRepo;
            _algoTaskRepo = algoTaskRepo;
            _fileRepo = fileRepo;
            _queue = queue;
        }

        public async Task<Result<Guid>> CreateAsync(CreateDetectionTaskDto dto, Guid userId)
        {
            try
            {
                // 基础校验
                if (dto.Images == null || !dto.Images.Any())
                    throw new Exception("请至少上传一张图片");

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
                foreach (var img in normalizedImages)
                {
                    var url = await _localfileService.LocalSaveAsync(img.File);
                    urls.Add(url);
                }

                // 创建DetectionTask的Domain对象并持久化
                var detectionTask = new DetectionTask(userId, urls, dto.Types);

                await _detectionTaskRepo.InsertAsync(detectionTask.ToEntity());

                // 领域内方法生成AlgoTask
                var algoTasks = detectionTask.GenerateAlgoTasks();

                // 持久化每个AlgoTask和对应的Localfile，并入队
                for (int i = 0; i < algoTasks.Count; i++)
                {
                    var task = algoTasks[i];
                    await _algoTaskRepo.InsertAsync(task.ToEntity());

                    var localFile = new Localfile
                    {
                        Id = UuidV7.NewUuidV7(),
                        UrlLocal = task.Url,
                        AlgoTaskId = task.Id,
                        Sid = i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _fileRepo.CreateLocalfileAsync(localFile);

                    // 入队
                    await _queue.EnqueueAsync(task.Id);
                }
                // 返回任务ID
                return Result<Guid>.Success(detectionTask.Id);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Fail(ResultCode.BusinessError, ex.Message);
            }
        }
    }
}

