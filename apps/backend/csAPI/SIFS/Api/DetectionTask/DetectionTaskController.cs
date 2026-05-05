using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.DetectionTaskApp;
using SIFS.Infrastructure.Identity;
using SIFS.Shared.Results;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/de-task")]
    [Authorize]
    public class DetectionTaskController : ControllerBase
    {
        private readonly ICurrentService _currentService;
        private readonly IDetectionTaskAppService _detectionTaskAppService;

        public DetectionTaskController(ICurrentService currentService, IDetectionTaskAppService detectionTaskAppService)
        {
            _currentService = currentService;
            _detectionTaskAppService = detectionTaskAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateDetectionTaskDto dto)
        {
            var uuid = _currentService.RequiredUuid;
            var result = await _detectionTaskAppService.CreateAsync(dto, uuid);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uuid = _currentService.RequiredUuid;
            var result = await _detectionTaskAppService.GetAllAsync(uuid);
            return ToActionResult(result);
        }

        [HttpGet("{guid:guid}")]
        public async Task<IActionResult> Get(Guid guid)
        {
            var uuid = _currentService.RequiredUuid;
            var result = await _detectionTaskAppService.GetAsync(guid, uuid);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(Result<T> result)
        {
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
