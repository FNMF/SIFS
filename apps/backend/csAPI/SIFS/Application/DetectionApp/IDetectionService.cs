using SIFS.Infrastructure.External;

namespace SIFS.Application.DetectionApp
{
    public interface IDetectionService
    {
        Task<List<DetectionResult>> DetectSelected(
            string imagePath,
            List<AiServiceType> types);
    }
}
