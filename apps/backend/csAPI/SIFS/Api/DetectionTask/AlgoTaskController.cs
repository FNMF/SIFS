using Microsoft.AspNetCore.Mvc;
using SIFS.Application.AlgoTaskApp;
using SIFS.Infrastructure.Identity;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/algo-task")]
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

            return BadRequest(result.Message);
        }
    }
}
