using SIFS.Infrastructure.External;

namespace SIFS.Application.DetectionApp
{
    public interface IDetectionService
    {
        Task<DetectionResult> DetectSelected(
            string imagePath,
            AiServiceType type);
    }
}
