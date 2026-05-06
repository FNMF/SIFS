namespace SIFS.Application.TaskManagement
{
    public class TaskManagementQuery
    {
        public Guid? UserId { get; set; }
        public int? AlgorithmId { get; set; }
        public string? AlgorithmName { get; set; }
        public int? Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? Failed { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
