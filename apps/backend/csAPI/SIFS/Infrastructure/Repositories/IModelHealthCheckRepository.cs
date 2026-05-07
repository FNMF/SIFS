using SIFS.Application.ModelHealthChecks;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IModelHealthCheckRepository
    {
        Task CreateAsync(ModelHealthCheck modelHealthCheck);
        Task<ModelHealthCheck?> GetLatestByAlgoModelIdAsync(int algoModelId);
        Task<Dictionary<int, ModelHealthCheck>> GetLatestByAlgoModelIdsAsync(IEnumerable<int> algoModelIds);
        Task<Paged<ModelHealthStatusDto>> ListLatestStatusesAsync(ModelHealthStatusQuery query);
        Task<ModelHealthSummaryDto> CountByLatestStatusAsync();
    }
}
