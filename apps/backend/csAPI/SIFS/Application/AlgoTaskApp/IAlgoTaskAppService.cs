using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public interface IAlgoTaskAppService
    {
        Task ExecuteAsync(Guid algoTaskId);
        Task<AlgoTaskExecutionResult> ExecuteCoreAsync(Guid algoTaskId);
        Task HandleExecutionSucceededAsync(Guid algoTaskId, AlgoTaskExecutionResult executionResult);
        Task HandleExecutionFailedAsync(Guid algoTaskId, string failureReason);
        Task<Result<AlgoTaskDetailDto>> GetDetailAsync(Guid algoTaskId, Guid userId);
    }

    public class AlgoTaskExecutionResult
    {
        public Guid TaskId { get; set; }
        public Guid AlgoTaskId { get; set; }
        public Guid UserId { get; set; }
        public string? Algorithm { get; set; }
        public string? ResultUrl { get; set; }
    }
}
