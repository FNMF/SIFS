using SIFS.Application.TaskAudits;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public interface ITaskAuditRepository
    {
        Task CreateAsync(TaskAudit taskAudit);
        Task<List<TaskAuditDto>> ListByTaskIdAsync(Guid taskId);
        Task<Dictionary<Guid, List<TaskAuditDto>>> ListByTaskIdsAsync(IEnumerable<Guid> taskIds);
    }
}
