using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public interface IAlgoTaskAppService
    {
        Task ExecuteAsync(Guid algoTaskId);
        Task<Result<AlgoTaskDetailDto>> GetDetailAsync(Guid algoTaskId, Guid userId);
    }
}
