namespace SIFS.Application.OperationLogs
{
    public class OperationLogDto
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string? ActorUsername { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string? TargetType { get; set; }
        public string? TargetId { get; set; }
        public string? RequestIp { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestSummary { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
