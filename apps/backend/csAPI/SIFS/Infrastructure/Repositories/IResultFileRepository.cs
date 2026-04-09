using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IResultFileRepository
    {
        Task InsertAsync(ResultFile resultFile);
        Task<Result<ResultFile>> GetByIdAsync(Guid id);
    }
}
