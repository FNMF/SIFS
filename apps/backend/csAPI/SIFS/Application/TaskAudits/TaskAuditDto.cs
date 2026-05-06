namespace SIFS.Application.TaskAudits
{
    public class TaskAuditDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public Guid? OperatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExtraJson { get; set; }
    }
}
