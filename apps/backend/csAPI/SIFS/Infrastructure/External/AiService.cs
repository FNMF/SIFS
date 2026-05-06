using SIFS.Application.AlgoModels;

namespace SIFS.Infrastructure.External
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAlgorithmEndpointResolver _algorithmEndpointResolver;

        public AiService(HttpClient httpClient, IAlgorithmEndpointResolver algorithmEndpointResolver)
        {
            _httpClient = httpClient;
            _algorithmEndpointResolver = algorithmEndpointResolver;
        }

        public async Task<DetectionResult> DetectAsync(AiServiceType type, string imageUrl, int? level, string? apiUrl = null)
        {
            var resolvedApiUrl = apiUrl;
            if (string.IsNullOrWhiteSpace(resolvedApiUrl))
            {
                var resolveResult = await _algorithmEndpointResolver.ResolveAsync(type);
                if (!resolveResult.IsSuccess)
                    throw new InvalidOperationException(resolveResult.Message);

                resolvedApiUrl = resolveResult.Data.ApiUrl;
            }

            if (string.IsNullOrWhiteSpace(resolvedApiUrl))
                throw new InvalidOperationException("algorithm API URL missing");

            var payload = new
            {
                image_url = imageUrl,
                level = level
            };

            var response = await _httpClient.PostAsJsonAsync(resolvedApiUrl, payload);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"algorithm returned non-2xx response: {(int)response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<DetectionResult>()
                ?? throw new InvalidOperationException("algorithm response parse failed");
        }
    }
}
