using Microsoft.Extensions.Options;

namespace SIFS.Infrastructure.External
{
    public class AiService:IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly AiServiceOptions _options;

        public AiService(HttpClient httpClient, IOptions<AiServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        // 传入 type，而不是写死
        public async Task<DetectionResult> DetectAsync(AiServiceType type, string imagePath)
        {
            if (!_options.Endpoints.TryGetValue(type, out var url))
                throw new NotSupportedException($"Unsupported type: {type}");

            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(
                await File.ReadAllBytesAsync(imagePath));

            form.Add(fileContent, "file", Path.GetFileName(imagePath));

            var response = await _httpClient.PostAsync(url, form);

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<DetectionResult>())!;
        }
    }
}
