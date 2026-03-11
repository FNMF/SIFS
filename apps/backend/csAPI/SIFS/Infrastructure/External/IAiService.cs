namespace SIFS.Infrastructure.External
{
    public interface IAiService
    {
        AiServiceType Type { get; }

        Task<DetectionResult> DetectAsync(string imagePath);
    }
}
