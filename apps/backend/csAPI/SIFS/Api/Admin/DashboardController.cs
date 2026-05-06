using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.Dashboard;
using SIFS.Infrastructure.Authorization;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize]
    [RequirePermission("admin:access")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(new
            {
                today_task_count = summary.TodayTaskCount,
                total_task_count = summary.TotalTaskCount,
                running_task_count = summary.RunningTaskCount,
                waiting_task_count = summary.WaitingTaskCount,
                failed_task_count = summary.FailedTaskCount,
                success_task_count = summary.SuccessTaskCount,
                algo_total_count = summary.AlgoTotalCount,
                algo_enabled_count = summary.AlgoEnabledCount,
                algo_offline_count = summary.AlgoOfflineCount
            });
        }

        [HttpGet("recent-tasks")]
        public async Task<IActionResult> GetRecentTasks([FromQuery] int limit = 10, [FromQuery] bool failedOnly = false, [FromQuery(Name = "failed_only")] bool? failedOnlySnake = null)
        {
            var items = await _dashboardService.GetRecentTasksAsync(limit, failedOnlySnake ?? failedOnly);
            return Ok(items.Select(x => new
            {
                task_id = x.TaskId,
                created_by_user_id = x.CreatedByUserId,
                created_by_username = x.CreatedByUsername,
                algorithm_name = x.AlgorithmName,
                status = x.Status,
                created_at = x.CreatedAt,
                started_at = x.StartedAt,
                finished_at = x.FinishedAt,
                failure_reason = x.FailureReason
            }));
        }

        [HttpGet("recent-failed-tasks")]
        public async Task<IActionResult> GetRecentFailedTasks([FromQuery] int limit = 10)
        {
            var items = await _dashboardService.GetRecentFailedTasksAsync(limit);
            return Ok(items.Select(x => new
            {
                task_id = x.TaskId,
                created_by_user_id = x.CreatedByUserId,
                created_by_username = x.CreatedByUsername,
                algorithm_name = x.AlgorithmName,
                status = x.Status,
                created_at = x.CreatedAt,
                started_at = x.StartedAt,
                finished_at = x.FinishedAt,
                failure_reason = x.FailureReason
            }));
        }

        [HttpGet("recent-logs")]
        public async Task<IActionResult> GetRecentLogs([FromQuery] int limit = 10)
        {
            var items = await _dashboardService.GetRecentLogsAsync(limit);
            return Ok(items.Select(x => new
            {
                id = x.Id,
                actor_id = x.ActorId,
                actor_username = x.ActorUsername,
                operation_type = x.OperationType,
                target_type = x.TargetType,
                target_id = x.TargetId,
                request_path = x.RequestPath,
                success = x.Success,
                failure_reason = x.FailureReason,
                created_at = x.CreatedAt
            }));
        }

        [HttpGet("task-status-count")]
        public async Task<IActionResult> GetTaskStatusCount()
        {
            var result = await _dashboardService.GetTaskStatusCountAsync();
            return Ok(new
            {
                items = result.Items.Select(x => new
                {
                    status = x.Status,
                    count = x.Count
                })
            });
        }

        [HttpGet("algo-status-count")]
        public async Task<IActionResult> GetAlgoStatusCount()
        {
            var result = await _dashboardService.GetAlgoStatusCountAsync();
            return Ok(new
            {
                total = result.Total,
                enabled = result.Enabled,
                disabled = result.Disabled,
                offline = result.Offline
            });
        }
    }
}
