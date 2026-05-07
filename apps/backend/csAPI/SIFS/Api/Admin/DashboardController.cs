using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.Dashboard;
using SIFS.Application.ModelHealthChecks;
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
                algo_offline_count = summary.AlgoOfflineCount,
                algo_timeout_count = summary.AlgoTimeoutCount,
                algo_online_count = summary.AlgoOnlineCount
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
                online = result.Online,
                offline = result.Offline,
                timeout = result.Timeout
            });
        }

        [HttpGet("algo-health")]
        [RequirePermission("algo:view")]
        public async Task<IActionResult> GetAlgoHealth([FromQuery] ModelHealthStatusQuery query)
        {
            var result = await _dashboardService.GetAlgoHealthAsync(query);
            return Ok(new
            {
                items = result.Data.Select(x => new
                {
                    algo_model_id = x.AlgoModelId,
                    name = x.Name,
                    enabled = x.Enabled,
                    api_url = x.ApiUrl,
                    health_status = x.HealthStatus,
                    response_time_ms = x.ResponseTimeMs,
                    checked_at = x.CheckedAt,
                    failure_reason = x.FailureReason,
                    description = x.Description
                }),
                total = result.Total,
                page = result.PageNumber,
                page_size = result.PageSize
            });
        }
    }
}
