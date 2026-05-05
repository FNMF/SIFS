using SIFS.Application.OperationLogs;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IOperationLogRepository
    {
        Task CreateAsync(OperationLog operationLog);

        Task<Paged<OperationLogDto>> QueryAsync(OperationLogQuery query);
    }
}
