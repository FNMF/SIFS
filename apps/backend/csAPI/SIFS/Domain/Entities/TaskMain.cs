using SIFS.Infrastructure.External;
using SIFS.Shared.Helpers;

namespace SIFS.Domain.Entities
{
    public class TaskMain
    {
        public Guid Id { get; private set; } = UuidV7.NewUuidV7();
        public int Status { get; private set; } = 0;        // 复合任务，值对应完成的子任务数量，初始为0，完成时为Urls.Count
        public bool IsCompleted => Status == Urls.Count;
        public List<string> Urls { get; private set; } = new();   // 复合任务，包含多个URL   
        public List<AiServiceType> Types { get; private set; }

        public TaskMain(List<string> urls, List<AiServiceType> types)
        {
            Urls = urls;
            Types = types;
        }
        public List<AlgoTask> GenerateAlgoTasks()
        {
            return Urls.Select(url => new AlgoTask(Id, url, Types)).ToList();
        }
        public bool OnAlgoTaskCompleted()
        {
            Status++;
            return IsCompleted;
        }
    }

    
}
