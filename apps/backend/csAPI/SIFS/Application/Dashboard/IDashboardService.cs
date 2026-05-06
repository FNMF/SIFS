namespace SIFS.Application.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<DashboardRecentTaskDto>> GetRecentTasksAsync(int limit, bool failedOnly = false);
        Task<List<DashboardRecentTaskDto>> GetRecentFailedTasksAsync(int limit);
        Task<List<DashboardRecentLogDto>> GetRecentLogsAsync(int limit);
        Task<DashboardTaskStatusCountDto> GetTaskStatusCountAsync();
        Task<DashboardAlgoStatusCountDto> GetAlgoStatusCountAsync();
    }
}
