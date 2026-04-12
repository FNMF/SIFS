namespace SIFS.Infrastructure.External
{
    public interface IAiService
    {
        Task<DetectionResult> DetectAsync(AiServiceType type, string imagePath, int? level);
    }
}
