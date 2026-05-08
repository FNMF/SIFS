namespace SIFS.Infrastructure.External
{
    public interface IAiService
    {
        Task<DetectionResult> DetectAsync(string imagePath, int? level, string apiUrl, string? algorithmName = null, Guid? userId = null);
    }
}
