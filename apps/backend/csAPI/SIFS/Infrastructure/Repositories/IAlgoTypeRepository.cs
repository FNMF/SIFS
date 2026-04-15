using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IAlgoTypeRepository
    {
        Task<Result<List<AlgoType>>> GetAllAsync();
    }
}
