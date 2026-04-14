using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Helpers;

namespace SIFS.Domain.Entities
{
    public class DetectionTask
    {
        public Guid Id { get; private set; } = UuidV7.NewUuidV7();
        public Guid UserId {  get; private set; }
        public int Status { get; private set; } = 0;        // 复合任务，值对应完成的子任务数量，初始为0，完成时为Urls.Count
        public bool IsCompleted => Status == Urls.Count;
        public List<string> Urls { get; private set; } = new();   // 复合任务，包含多个URL   
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public List<AiServiceType> Types { get; private set; }
        public int? Level { get; private set; }

        public DetectionTask(Guid userid, List<string> urls, List<AiServiceType> types, int? level)
        {
            UserId = userid;                                                                                                                                                                 
            Urls = urls;
            Types = types;
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
            return Urls.SelectMany(url => Types.Select(type => new TaskItem(Id, url, type, Level))).ToList();
        }
        public bool OnAlgoTaskCompleted()
        {
            Status++;
            return IsCompleted;
        }
    }

    
}
