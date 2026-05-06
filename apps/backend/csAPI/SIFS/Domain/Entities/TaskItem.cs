using SIFS.Domain.Enum;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; private set; } = UuidV7.NewUuidV7();
        public Guid TaskId { get; private set; }
        public string Url { get; private set; }
        public AiServiceType Type { get; private set; }
        public int? AlgoModelId { get; private set; }
        public string? AlgoName { get; private set; }
        public string? AlgoApiUrl { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? FinishedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public int? Level { get; private set; }
        public AlgoTaskStatus Status { get; private set; } = AlgoTaskStatus.pending;
        public string Description { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Result<DetectionResult>? Result { get; private set; }

        public TaskItem(Guid taskId, string url, AiServiceType type, int? level)
        {
            TaskId = taskId;
            Url = url;
            Type = type;
            AlgoName = type.ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Level = level;
        }
        public void SetAlgorithmEndpoint(int? algoModelId, string algoName, string algoApiUrl)
        {
            AlgoModelId = algoModelId;
            AlgoName = algoName;
            AlgoApiUrl = algoApiUrl;
        }
        public AlgoTask ToEntity()
        {
            return new AlgoTask
            {
                Id = Id,
                TaskId = TaskId,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Status = (int)Status,
                AlgoModelId = AlgoModelId,
                AlgoName = AlgoName,
                AlgoApiUrl = AlgoApiUrl,
                FailureReason = FailureReason,
                StartedAt = StartedAt,
                FinishedAt = FinishedAt,
                DeletedAt = DeletedAt
            };
        }
        public void MarkAsRunning()
        {
            if(Status == AlgoTaskStatus.pending)
            {
                Status = AlgoTaskStatus.running;
                UpdatedAt = DateTime.UtcNow;
                StartedAt ??= UpdatedAt;
            }
        }
        public void MarkAsDone(DetectionResult result)
        {
            if(Status == AlgoTaskStatus.running)
            {
                Status = AlgoTaskStatus.done;
                Result = Result<DetectionResult>.Success(result);
                UpdatedAt = DateTime.UtcNow;
                FinishedAt = UpdatedAt;
            }
        }
        public void MarkAsFailed(string? failureReason = null)
        {
            Status = AlgoTaskStatus.failed;
            FailureReason = failureReason;
            UpdatedAt = DateTime.UtcNow;
            FinishedAt = UpdatedAt;
        }
    }
}
