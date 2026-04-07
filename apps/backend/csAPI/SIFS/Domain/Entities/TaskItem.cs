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
        public List<AiServiceType> Types { get; private set; }
        public AlgoTaskStatus Status { get; private set; } = AlgoTaskStatus.pending;
        public string Description { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Result<List<DetectionResult>>? Result { get; private set; }

        public TaskItem(Guid taskId, string url, List<AiServiceType> types)
        {
            TaskId = taskId;
            Url = url;
            Types = types;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public AlgoTask ToEneity()
        {
            return new AlgoTask
            {
                Id = Id,
                TaskId = TaskId,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
        public void MarkAsRunning()
        {
            if(Status == AlgoTaskStatus.pending)
            {
                Status = AlgoTaskStatus.running;
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public void MarkAsDone(List<DetectionResult> result)
        {
            if(Status == AlgoTaskStatus.running)
            {
                Status = AlgoTaskStatus.done;
                Result = Result<List<DetectionResult>>.Success(result);
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public void MarkAsFailed()
        {
            Status = AlgoTaskStatus.failed;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
