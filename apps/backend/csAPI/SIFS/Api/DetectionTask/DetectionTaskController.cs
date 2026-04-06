using Microsoft.AspNetCore.Mvc;
using SIFS.Infrastructure.Identity;

namespace SIFS.Api.DetectionTask
{
    [ApiController]
    [Route("api/de-task")]
    public class DetectionTaskController:ControllerBase
    {
        private readonly ICurrentService _currentService;
        public DetectionTaskController(ICurrentService currentService)
        {
            _currentService = currentService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateDetectionTaskDto dto)
        {
            if (dto.Images == null || !dto.Images.Any())
                throw new Exception("至少上传一张图片");

            // 防重复
            if (dto.Images.Select(x => x.Order).Distinct().Count() != dto.Images.Count)
                throw new Exception("排序重复");

            //重排
            var normalized = dto.Images
                .OrderBy(x => x.Order)
                .Select((x, i) => new { x.File, Order = i });

            var uuid = _currentService.RequiredUuid;



        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uuid = _currentService.RequiredUuid;
        }

    }
}
