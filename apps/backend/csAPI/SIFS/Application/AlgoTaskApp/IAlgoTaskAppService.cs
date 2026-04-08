namespace SIFS.Application.AlgoTaskApp
{
    public interface IAlgoTaskAppService
    {
        Task ExecuteAsync(Guid algoTaskId);
    }
}
