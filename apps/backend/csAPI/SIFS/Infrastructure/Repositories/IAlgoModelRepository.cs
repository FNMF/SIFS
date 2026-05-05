using SIFS.Application.AlgoModels;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IAlgoModelRepository
    {
        Task<AlgoModel?> FindByIdAsync(int id, bool includeDeleted = false);
        Task<AlgoModel?> FindByNameAsync(string name, bool includeDeleted = false);
        Task CreateAsync(AlgoModel model);
        Task UpdateAsync(AlgoModel model);
        Task<Paged<AlgoModelDto>> ListAsync(AlgoModelQuery query);
    }
}
