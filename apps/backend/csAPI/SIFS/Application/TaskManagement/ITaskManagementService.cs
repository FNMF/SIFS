using SIFS.Shared.Results;

namespace SIFS.Application.TaskManagement
{
    public interface ITaskManagementService
    {
        Task<Result<Paged<TaskManagementListItemDto>>> QueryAdminAsync(TaskManagementQuery query, Guid actorId);
        Task<Result<Paged<TaskManagementListItemDto>>> QueryUserAsync(TaskManagementQuery query, Guid userId);
        Task<Result<TaskManagementDetailDto>> GetAdminDetailAsync(Guid taskId, Guid actorId);
        Task<Result<TaskManagementDetailDto>> GetUserDetailAsync(Guid taskId, Guid userId);
        Task<Result<List<TaskStatusFlowItemDto>>> GetAdminStatusFlowAsync(Guid taskId, Guid actorId);
        Task<Result<TaskOperationResultDto>> CancelAdminAsync(Guid taskId, Guid actorId);
        Task<Result<TaskOperationResultDto>> RetryAdminAsync(Guid taskId, Guid actorId);
        Task<Result<TaskOperationResultDto>> DeleteAdminAsync(Guid taskId, Guid actorId);
    }
}
