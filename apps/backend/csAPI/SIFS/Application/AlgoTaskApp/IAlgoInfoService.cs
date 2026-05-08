using SIFS.Application.AlgoModels;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public interface IAlgoInfoService
    {
        Task<Result<List<AlgoModelDto>>> GetAllAsync();
    }
}
