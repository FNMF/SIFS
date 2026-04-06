namespace SIFS.Application.DetectionTask
{
    public class AlgoTaskAppService: IAlgoTaskAppService
    {
        public async Task ExecuteAsync(Guid algoTaskId)
        {
            var task = await _repo.GetById(algoTaskId);

            task.MarkAsRunning();

            var result = await _detectionService.DetectSelected(task.Url, task.Types);

            task.MarkAsDone(result);

            await _repo.Update(task);

            // 判断父任务是否完成
            var parent = await _taskRepo.GetById(task.TaskId);
            var completed = parent.OnAlgoTaskCompleted();

            if (completed)
            {
                // SignalR / 通知
            }
        }
    }
}
