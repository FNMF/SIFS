using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.OperationLogs;
using SIFS.Infrastructure.Authorization;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/operation-logs")]
    [Authorize]
    [RequirePermission("log:view")]
    public class OperationLogsController : ControllerBase
    {
        private readonly IOperationLogService _operationLogService;

        public OperationLogsController(IOperationLogService operationLogService)
        {
            _operationLogService = operationLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] OperationLogQuery query)
        {
            var result = await _operationLogService.QueryLogsAsync(query);

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

            return BadRequest(result.Message);
        }
    }
}
