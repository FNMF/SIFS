using Microsoft.EntityFrameworkCore;
using SIFS.Application.OperationLogs;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class OperationLogRepository : IOperationLogRepository
    {
        private readonly SIFSContext _context;

        public OperationLogRepository(SIFSContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(OperationLog operationLog)
        {
            await _context.OperationLogs.AddAsync(operationLog);
            await _context.SaveChangesAsync();
        }

        public async Task<Paged<OperationLogDto>> QueryAsync(OperationLogQuery query)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var logs = _context.OperationLogs.AsNoTracking().AsQueryable();

            if (query.ActorId.HasValue)
                logs = logs.Where(x => x.ActorId == query.ActorId.Value);

            if (!string.IsNullOrWhiteSpace(query.ActorUsername))
                logs = logs.Where(x => x.ActorUsername == query.ActorUsername.Trim());

            if (!string.IsNullOrWhiteSpace(query.OperationType))
                logs = logs.Where(x => x.OperationType == query.OperationType.Trim());

            if (query.Success.HasValue)
                logs = logs.Where(x => x.Success == query.Success.Value);

            if (query.StartTime.HasValue)
                logs = logs.Where(x => x.CreatedAt >= query.StartTime.Value);

            if (query.EndTime.HasValue)
                logs = logs.Where(x => x.CreatedAt <= query.EndTime.Value);

            if (!string.IsNullOrWhiteSpace(query.TargetType))
                logs = logs.Where(x => x.TargetType == query.TargetType.Trim());

            var total = await logs.CountAsync();
            var items = await logs
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OperationLogDto
                {
                    Id = x.Id,
                    ActorId = x.ActorId,
                    ActorUsername = x.ActorUsername,
                    OperationType = x.OperationType,
                    TargetType = x.TargetType,
                    TargetId = x.TargetId,
                    RequestIp = x.RequestIp,
                    RequestMethod = x.RequestMethod,
                    RequestPath = x.RequestPath,
                    RequestSummary = x.RequestSummary,
                    Success = x.Success,
                    FailureReason = x.FailureReason,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new Paged<OperationLogDto>
            {
                Data = items,
                Total = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
