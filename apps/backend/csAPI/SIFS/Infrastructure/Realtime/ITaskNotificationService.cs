namespace SIFS.Infrastructure.Realtime
{
    public interface ITaskNotificationService
    {
        Task NotifyAlgoTaskFinishedAsync(TaskFinishedNotification notification);
    }
}
