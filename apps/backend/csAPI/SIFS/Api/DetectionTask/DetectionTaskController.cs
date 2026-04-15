using Microsoft.AspNetCore.Mvc;
using SIFS.Application.DetectionTaskApp;
using SIFS.Infrastructure.Identity;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/de-task")]
    public class DetectionTaskController:ControllerBase
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
            if(result.IsSuccess)
                return Ok(result.Data);
            else
                return BadRequest(result.Message);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uuid = _currentService.RequiredUuid;

            //TODO
            return Ok;
        }
        
    }
}
