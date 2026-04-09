using SIFS.Infrastructure.External;

namespace SIFS.Application.DetectionApp
{
    public class DetectionService : IDetectionService
    {
        private readonly IAiService _service;

        public DetectionService(IAiService service)
        {
            _service = service;
        }

        public async Task<DetectionResult> DetectSelected(
            string imagePath,
            AiServiceType type)
        {
            var result = await _service.DetectAsync(type,imagePath);

            // 持久化结果到数据库

            return result;
        }
    }
}
