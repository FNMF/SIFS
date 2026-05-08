namespace SIFS.Infrastructure
{
    public class AlgoTaskQueueItem
    {
        public Guid TaskId { get; set; }
        public int AlgoModelId { get; set; }

        public AlgoTaskQueueItem()
        {
        }

        public AlgoTaskQueueItem(Guid taskId, int algoModelId)
        {
            TaskId = taskId;
            AlgoModelId = algoModelId;
        }
    }
}
