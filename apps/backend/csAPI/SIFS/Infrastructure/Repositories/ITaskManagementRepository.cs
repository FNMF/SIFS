using SIFS.Application.TaskManagement;
using SIFS.Infrastructure;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface ITaskManagementRepository
    {
        Task<Paged<TaskManagementListItemDto>> QueryAsync(TaskManagementQuery query, Guid? restrictToUserId = null);
        Task<TaskManagementDetailDto?> GetDetailAsync(Guid taskId, Guid? restrictToUserId = null, bool includeDeleted = false);
        Task<List<TaskStatusFlowItemDto>?> GetStatusFlowAsync(Guid taskId, Guid? restrictToUserId = null);
        Task<bool> ExistsForUserAsync(Guid taskId, Guid userId);
        Task CancelAsync(Guid taskId, string reason);
        Task SoftDeleteAsync(Guid taskId, string reason);
        Task<(Guid NewTaskId, List<AlgoTaskQueueItem> AlgoTasks)> RetryAsync(Guid taskId, Dictionary<Guid, SIFS.Infrastructure.External.AlgorithmEndpointResolution> algorithmEndpoints);
    }
}
