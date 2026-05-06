namespace SIFS.Application.TaskManagement
{
    public class TaskManagementListItemDto
    {
        public Guid TaskId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public int SubTaskCount { get; set; }
        public int CompletedSubTaskCount { get; set; }
        public int FailedSubTaskCount { get; set; }
        public string? AlgorithmName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public double? Duration { get; set; }
        public string? FailureReason { get; set; }
        public string? PreviewImageUrl { get; set; }
    }

    public class TaskManagementDetailDto
    {
        public Guid TaskId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
        public int? AlgorithmId { get; set; }
        public string? AlgorithmName { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string? OriginalImagePath { get; set; }
        public List<string> OriginalImagePaths { get; set; } = new();
        public string? ResultPath { get; set; }
        public List<string> ResultPaths { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public double? Duration { get; set; }
        public string? FailureReason { get; set; }
        public List<TaskManagementSubTaskDto> SubTasks { get; set; } = new();
    }

    public class TaskManagementSubTaskDto
    {
        public Guid TaskId { get; set; }
        public int? AlgorithmId { get; set; }
        public int? AlgorithmTypeId { get; set; }
        public string? AlgorithmName { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? OriginalImagePath { get; set; }
        public string? ResultPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public double? Duration { get; set; }
        public string? FailureReason { get; set; }
    }

    public class TaskStatusFlowItemDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }
    }

    public class TaskOperationResultDto
    {
        public Guid TaskId { get; set; }
        public Guid? NewTaskId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
