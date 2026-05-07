using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Application.ModelHealthChecks
{
    public interface IModelHealthCheckService
    {
        Task CheckAlgoHealthAsync(AlgoModel algoModel);
        Task CheckEnabledAlgosHealthAsync();
        Task<ModelHealthStatusDto?> GetLatestAlgoHealthAsync(int algoModelId);
        Task<ModelHealthSummaryDto> GetDashboardAlgoHealthSummaryAsync();
        Task<Paged<ModelHealthStatusDto>> ListAlgoHealthStatusesAsync(ModelHealthStatusQuery query);
    }
}
