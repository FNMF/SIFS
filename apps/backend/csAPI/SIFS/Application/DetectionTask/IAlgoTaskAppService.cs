namespace SIFS.Application.DetectionTask
{
    public interface IAlgoTaskAppService
    {
        Task ExecuteAsync(Guid algoTaskId);
    }
}
