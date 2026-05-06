using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.TaskManagement;
using SIFS.Infrastructure.Authorization;
using SIFS.Infrastructure.Identity;
using SIFS.Shared.Results;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskManagementService _taskManagementService;
        private readonly ICurrentService _currentService;

        public TasksController(
            ITaskManagementService taskManagementService,
            ICurrentService currentService)
        {
            _taskManagementService = taskManagementService;
            _currentService = currentService;
        }

        [HttpGet]
        [RequirePermission("task:view:all")]
        public async Task<IActionResult> Query([FromQuery] TaskManagementQuery query)
        {
            var result = await _taskManagementService.QueryAdminAsync(query, _currentService.RequiredUuid);
            return ToPagedActionResult(result);
        }

        [HttpGet("{taskId:guid}")]
        [RequirePermission("task:view:all")]
        public async Task<IActionResult> Get(Guid taskId)
        {
            var result = await _taskManagementService.GetAdminDetailAsync(taskId, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpGet("{taskId:guid}/status-flow")]
        [RequirePermission("task:view:all")]
        public async Task<IActionResult> GetStatusFlow(Guid taskId)
        {
            var result = await _taskManagementService.GetAdminStatusFlowAsync(taskId, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpPost("{taskId:guid}/cancel")]
        [RequirePermission("task:delete")]
        public async Task<IActionResult> Cancel(Guid taskId)
        {
            var result = await _taskManagementService.CancelAdminAsync(taskId, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpPost("{taskId:guid}/retry")]
        [RequirePermission("task:retry")]
        public async Task<IActionResult> Retry(Guid taskId)
        {
            var result = await _taskManagementService.RetryAdminAsync(taskId, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpDelete("{taskId:guid}")]
        [RequirePermission("task:delete")]
        public async Task<IActionResult> Delete(Guid taskId)
        {
            var result = await _taskManagementService.DeleteAdminAsync(taskId, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        private IActionResult ToPagedActionResult<T>(Result<Paged<T>> result)
        {
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    items = result.Data.Data,
                    total = result.Data.Total,
                    page = result.Data.PageNumber,
                    page_size = result.Data.PageSize
                });
            }

            return ToFailureActionResult(result.Code, result.Message);
        }

        private IActionResult ToActionResult<T>(Result<T> result)
        {
            return result.IsSuccess
                ? Ok(result.Data)
                : ToFailureActionResult(result.Code, result.Message);
        }

        private IActionResult ToFailureActionResult(ResultCode code, string message)
        {
            return code switch
            {
                ResultCode.Unauthorized => Unauthorized(message),
                ResultCode.Forbidden => Forbid(),
                ResultCode.NotFound => NotFound(message),
                _ => BadRequest(message)
            };
        }
    }
}
