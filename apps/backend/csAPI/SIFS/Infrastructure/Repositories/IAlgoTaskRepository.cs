using SIFS.Application.AlgoTaskApp;
using SIFS.Application.DetectionTaskApp;
using SIFS.Domain.Entities;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IAlgoTaskRepository
    {
        Task<Result<AlgoTask>> GetTaskByIdAsync(Guid id);
        Task<Result<TaskItem>> GetAggregateByGuidAsync(Guid id);
        Task<List<AlgoReadDto>> GetAllReadDtosByTaskIdAsync(Guid taskId);
        Task<AlgoTaskDetailDto?> GetDetailDtoByIdAsync(Guid algoTaskId, Guid userId);
        Task InsertAsync(AlgoTask algoTask);
        Task UpdateAsync(AlgoTask algoTask);
    }
}
