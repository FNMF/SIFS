namespace SIFS.Application.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TodayTaskCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int RunningTaskCount { get; set; }
        public int WaitingTaskCount { get; set; }
        public int FailedTaskCount { get; set; }
        public int SuccessTaskCount { get; set; }
        public int AlgoTotalCount { get; set; }
        public int AlgoEnabledCount { get; set; }
        public int AlgoOfflineCount { get; set; }
        public int AlgoTimeoutCount { get; set; }
        public int AlgoOnlineCount { get; set; }
    }

    public class DashboardRecentTaskDto
    {
        public Guid TaskId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
        public string? AlgorithmName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? FailureReason { get; set; }
    }

    public class DashboardRecentLogDto
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string? ActorUsername { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string? TargetType { get; set; }
        public string? TargetId { get; set; }
        public string? RequestPath { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardStatusCountItemDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardTaskStatusCountDto
    {
        public List<DashboardStatusCountItemDto> Items { get; set; } = new();
    }

    public class DashboardAlgoStatusCountDto
    {
        public int Total { get; set; }
        public int Enabled { get; set; }
        public int Disabled { get; set; }
        public int Online { get; set; }
        public int Offline { get; set; }
        public int Timeout { get; set; }
    }
}
