namespace SIFS.Infrastructure.Realtime
{
    public class TaskFinishedNotification
    {
        public Guid UserId { get; set; }
        public Guid TaskId { get; set; }
        public Guid AlgoTaskId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string? Algorithm { get; set; }
        public string? ResultUrl { get; set; }
        public string? FailureReason { get; set; }
        public bool ParentTaskCompleted { get; set; }
        public DateTime FinishedAt { get; set; } = DateTime.UtcNow;
    }
}
