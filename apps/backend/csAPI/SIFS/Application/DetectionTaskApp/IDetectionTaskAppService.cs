using SIFS.Api.DetectionTask;
using SIFS.Shared.Results;

namespace SIFS.Application.DetectionTaskApp
{
    public interface IDetectionTaskAppService
    {
        Task<Result<Guid>> CreateAsync(CreateDetectionTaskDto dto, Guid userId);
        Task<Result<List<DetectionTaskReadDto>>> GetAllAsync(Guid userId);
        Task<Result<DetectionTaskDetailDto>> GetAsync(Guid guid, Guid userId);
    }
}
