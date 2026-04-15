using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public interface IAlgoInfoService
    {
        Task<Result<List<AlgoType>>> GetAllAsync();
    }
}
