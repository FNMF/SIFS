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
        public async Task<DetectionResult> DetectAsync(AiServiceType type, string imageUrl, int? level)
        {
            if (!_options.Endpoints.TryGetValue(type, out var url))
                throw new NotSupportedException($"Unsupported type: {type}");

            var payload = new
            {
                image_url = imageUrl,
                level = level
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<DetectionResult>())!;
        }
    }
}
