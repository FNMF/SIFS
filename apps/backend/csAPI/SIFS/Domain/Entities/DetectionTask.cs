using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Helpers;

namespace SIFS.Domain.Entities
{
    public class DetectionTask
    {
        public Guid Id { get; private set; } = UuidV7.NewUuidV7();
        public Guid UserId { get; private set; }
        public int Status { get; private set; } = 0;
        public bool IsCompleted => Status == Urls.Count * Algorithms.Count;
        public List<string> Urls { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public List<AlgorithmRef> Algorithms { get; private set; }
        public int? Level { get; private set; }

        public DetectionTask(Guid userid, List<string> urls, List<AlgorithmRef> algorithms, int? level)
        {
            UserId = userid;
            Urls = urls;
            Algorithms = algorithms;
            Level = level;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public TaskList ToEntity()
        {
            return new TaskList
            {
                Id = Id,
                UserId = UserId,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Level = Level
            };
        }

        public List<TaskItem> GenerateAlgoTasks()
        {
            return Urls.SelectMany(url => Algorithms.Select(algorithm => new TaskItem(Id, url, algorithm, Level))).ToList();
        }

        public void SetCompletedSubTaskCount(int completedCount)
        {
            Status = Math.Max(completedCount, 0);
        }
    }

    public class AlgorithmRef
    {
        public int? AlgoModelId { get; set; }
        public string AlgoName { get; set; } = string.Empty;
        public string AlgoApiUrl { get; set; } = string.Empty;
    }
}
