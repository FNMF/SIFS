using SIFS.Application.Detection;
using SIFS.Domain.Entities;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.External;
using SIFS.Shared.Results;
using System.Threading.Tasks;

namespace SIFS.Application.Task
{
    public class TaskQueueService
    {
        private readonly Queue<AlgoTask> _queue = new();
        private readonly object _lock = new();
        private bool _isRunning = false;

        private readonly IDetectionService _detectionService;

        public TaskQueueService(IDetectionService detectionService)
        {
            _detectionService = detectionService;
        }

        public void Enqueue(AlgoTask algoTask)
        {
            lock (_lock)
            {
                _queue.Enqueue(algoTask);

                if (!_isRunning)
                {
                    _isRunning = true;
                    _ = ProcessQueue();
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessQueue()
        {
            while (true)
            {
                AlgoTask? task = null;

                lock (_lock)
                {
                    if (_queue.Count > 0)
                        task = _queue.Dequeue();
                    else
                    {
                        _isRunning = false;
                        break;
                    }
                }

                try
                {
                    task.MarkAsRunning();

                    var result = await _detectionService
                        .DetectSelected(task.Url, task.Types);

                    task.MarkAsDone(result);

                    // 手动“触发领域事件”
                    await HandleAlgoTaskCompleted(task);
                }
                catch
                {
                    task.MarkAsFailed();
                }
            }

            _isRunning = false;
        }

        private async System.Threading.Tasks.Task HandleAlgoTaskCompleted(AlgoTask algoTask)
        {
            /*var task = await _taskRepository.GetById(algoTask.TaskId);

            var isCompleted = task.OnAlgoTaskCompleted();

            await _taskRepository.Update(task);

            if (isCompleted)
            {
                // 在这里处理“任务完成”
                await OnTaskCompleted(task);
            }*/
        }
    }
}
