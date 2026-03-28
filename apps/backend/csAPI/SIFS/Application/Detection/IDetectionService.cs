using SIFS.Infrastructure.External;

namespace SIFS.Application.Detection
{
    public interface IDetectionService
    {
        Task<List<DetectionResult>> DetectSelected(
            string imagePath,
            List<AiServiceType> types);
    }
}
