using Microsoft.AspNetCore.SignalR;

namespace SIFS.Infrastructure.Realtime
{
    public class TaskNotificationService : ITaskNotificationService
    {
        private readonly IHubContext<TaskNotificationHub> _hubContext;
        private readonly ILogger<TaskNotificationService> _logger;

        public TaskNotificationService(
            IHubContext<TaskNotificationHub> hubContext,
            ILogger<TaskNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyAlgoTaskFinishedAsync(TaskFinishedNotification notification)
        {
            try
            {
                var payload = new
                {
                    @event = "algo_task.finished",
                    task_id = notification.TaskId,
                    algo_task_id = notification.AlgoTaskId,
                    status = notification.Status,
                    status_text = notification.StatusText,
                    algorithm = notification.Algorithm,
                    result_url = notification.ResultUrl,
                    failure_reason = notification.FailureReason,
                    parent_task_completed = notification.ParentTaskCompleted,
                    finished_at = notification.FinishedAt
                };

                await _hubContext.Clients
                    .Group(TaskNotificationHub.UserGroup(notification.UserId))
                    .SendAsync("algo_task.finished", payload);

                await _hubContext.Clients
                    .Group(TaskNotificationHub.UserGroup(notification.UserId))
                    .SendAsync("task.message", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Task notification push failed. TaskId={TaskId}, AlgoTaskId={AlgoTaskId}",
                    notification.TaskId,
                    notification.AlgoTaskId);
            }
        }
    }
}
