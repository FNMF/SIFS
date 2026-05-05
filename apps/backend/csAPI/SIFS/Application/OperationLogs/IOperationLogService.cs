using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Results;

namespace SIFS.Application.OperationLogs
{
    public interface IOperationLogService
    {
        Task RecordFromEventAsync(AppEvent appEvent);

        Task<Result<Paged<OperationLogDto>>> QueryLogsAsync(OperationLogQuery query);
    }
}
