using Microsoft.EntityFrameworkCore;
using SIFS.Application.TaskAudits;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskAuditRepository : ITaskAuditRepository
    {
        private readonly SIFSContext _context;

        public TaskAuditRepository(SIFSContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(TaskAudit taskAudit)
        {
            await _context.TaskAudits.AddAsync(taskAudit);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskAuditDto>> ListByTaskIdAsync(Guid taskId)
        {
            return await _context.TaskAudits
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => Map(x))
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, List<TaskAuditDto>>> ListByTaskIdsAsync(IEnumerable<Guid> taskIds)
        {
            var ids = taskIds.Distinct().ToList();
            var audits = await _context.TaskAudits
                .AsNoTracking()
                .Where(x => ids.Contains(x.TaskId))
                .OrderBy(x => x.CreatedAt)
                .Select(x => Map(x))
                .ToListAsync();

            return audits
                .GroupBy(x => x.TaskId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        private static TaskAuditDto Map(TaskAudit audit)
        {
            return new TaskAuditDto
            {
                Id = audit.Id,
                TaskId = audit.TaskId,
                FromStatus = audit.FromStatus,
                ToStatus = audit.ToStatus,
                Reason = audit.Reason,
                OperatorId = audit.OperatorId,
                CreatedAt = audit.CreatedAt,
                ExtraJson = audit.ExtraJson
            };
        }
    }
}
