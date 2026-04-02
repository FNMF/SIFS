using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface ILocalfileRepository
    {
        Task<Result<Localfile>> GetLocalfileByIdAsync(Guid id);
        Task<Result<List<Localfile>>> GetLocalfilesByAlgoTaskIdAsync(Guid algoId);
        Task<Result<Localfile>> CreateLocalfileAsync(Localfile localfile);
        Task<Result<List<Localfile>>> CreateLocalfilesAsync(List<Localfile> localfiles);
    }
}
