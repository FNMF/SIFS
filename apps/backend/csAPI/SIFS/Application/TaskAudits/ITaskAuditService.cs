namespace SIFS.Application.TaskAudits
{
    public interface ITaskAuditService
    {
        Task RecordTransitionAsync(
            Guid taskId,
            string? fromStatus,
            string toStatus,
            string? reason = null,
            Guid? operatorId = null,
            object? extra = null);

        Task<List<TaskAuditDto>> ListByTaskIdAsync(Guid taskId);
    }
}
