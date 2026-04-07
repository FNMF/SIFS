using SIFS.Api.DetectionTask;
using SIFS.Shared.Results;

namespace SIFS.Application.DetectionTaskApp
{
    public interface IDetectionTaskAppService
    {
        Task<Result<Guid>> CreateAsync(CreateDetectionTaskDto dto, Guid userId);
    }
}
