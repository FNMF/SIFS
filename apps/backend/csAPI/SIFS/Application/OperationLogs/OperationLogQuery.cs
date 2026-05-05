namespace SIFS.Application.OperationLogs
{
    public class OperationLogQuery
    {
        public Guid? ActorId { get; set; }
        public string? ActorUsername { get; set; }
        public string? OperationType { get; set; }
        public bool? Success { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? TargetType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
