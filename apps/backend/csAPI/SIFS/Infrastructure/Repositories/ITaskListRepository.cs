using SIFS.Application.DetectionTaskApp;
using SIFS.Domain.Entities;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface ITaskListRepository
    {
        Task<Result<TaskList>> GetTaskListByIdAsync(Guid id);
        Task<Result<DetectionTask>> GetDetectionTaskAggregateByGuidAsync(Guid id);
        Task<List<DetectionTaskReadDto>> GetAllReadDtosByUserIdAsync(Guid userId);
        Task InsertAsync(TaskList taskList);
        Task UpdateAsync(TaskList taskList);
    }
}
