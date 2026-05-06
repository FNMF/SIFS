using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.TaskManagement;
using SIFS.Infrastructure.Identity;
using SIFS.Shared.Results;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/tasks")]
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
        public async Task<IActionResult> Query([FromQuery] TaskManagementQuery query)
        {
            var result = await _taskManagementService.QueryUserAsync(query, _currentService.RequiredUuid);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new
            {
                items = result.Data.Data,
                total = result.Data.Total,
                page = result.Data.PageNumber,
                page_size = result.Data.PageSize
            });
        }

        [HttpGet("{taskId:guid}")]
        public async Task<IActionResult> Get(Guid taskId)
        {
            var result = await _taskManagementService.GetUserDetailAsync(taskId, _currentService.RequiredUuid);
            if (result.IsSuccess)
                return Ok(result.Data);

            return result.Code switch
            {
                ResultCode.Forbidden => Forbid(),
                ResultCode.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }
    }
}
