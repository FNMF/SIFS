using System.Text.Json;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;

namespace SIFS.Application.TaskAudits
{
    public class TaskAuditService : ITaskAuditService
    {
        private readonly ITaskAuditRepository _taskAuditRepository;
        private readonly ILogger<TaskAuditService> _logger;

        public TaskAuditService(
            ITaskAuditRepository taskAuditRepository,
            ILogger<TaskAuditService> logger)
        {
            _taskAuditRepository = taskAuditRepository;
            _logger = logger;
        }

        public async Task RecordTransitionAsync(
            Guid taskId,
            string? fromStatus,
            string toStatus,
            string? reason = null,
            Guid? operatorId = null,
            object? extra = null)
        {
            try
            {
                var audit = new TaskAudit
                {
                    Id = UuidV7.NewUuidV7(),
                    TaskId = taskId,
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    Reason = reason,
                    OperatorId = operatorId,
                    CreatedAt = DateTime.UtcNow,
                    ExtraJson = extra == null ? null : JsonSerializer.Serialize(extra)
                };

                await _taskAuditRepository.CreateAsync(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record task audit. TaskId={TaskId}, ToStatus={ToStatus}", taskId, toStatus);
            }
        }

        public Task<List<TaskAuditDto>> ListByTaskIdAsync(Guid taskId)
        {
            return _taskAuditRepository.ListByTaskIdAsync(taskId);
        }
    }
}
