using SIFS.Infrastructure.External;

namespace SIFS.Application.Detection
{
    public class DetectionService : IDetectionService
    {
        private readonly IEnumerable<IAiService> _services;

        public DetectionService(IEnumerable<IAiService> services)
        {
            _services = services;
        }

        public async Task<List<DetectionResult>> DetectSelected(
            string imagePath,
            List<AiServiceType> types)
        {
            var services = _services
                .Where(x => types.Contains(x.Type));

            var tasks = services.Select(x => x.DetectAsync(imagePath));

            var results = await System.Threading.Tasks.Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}
