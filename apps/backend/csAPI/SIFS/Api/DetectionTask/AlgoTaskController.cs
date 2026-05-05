using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.AlgoTaskApp;
using SIFS.Infrastructure.Identity;
using SIFS.Shared.Results;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/algo-task")]
    [Authorize]
    public class AlgoTaskController : ControllerBase
    {
        private readonly IAlgoTaskAppService _algoTaskAppService;
        private readonly ICurrentService _currentService;

        public AlgoTaskController(IAlgoTaskAppService algoTaskAppService, ICurrentService currentService)
        {
            _algoTaskAppService = algoTaskAppService;
            _currentService = currentService;
        }

        [HttpGet("{guid:guid}")]
        public async Task<IActionResult> Get(Guid guid)
        {
            var userId = _currentService.RequiredUuid;
            var result = await _algoTaskAppService.GetDetailAsync(guid, userId);

            if (result.IsSuccess)
                return Ok(result.Data);

            return result.Code switch
            {
                ResultCode.Unauthorized => Unauthorized(result.Message),
                ResultCode.Forbidden => Forbid(),
                ResultCode.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }
    }
}
