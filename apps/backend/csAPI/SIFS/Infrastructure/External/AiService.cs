namespace SIFS.Infrastructure.External
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DetectionResult> DetectAsync(string imageUrl, int? level, string apiUrl, string? algorithmName = null, Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                throw new InvalidOperationException("algorithm API URL missing");

            var payload = new
            {
                image_url = imageUrl,
                level = level,
                algorithm = algorithmName,
                user_id = userId?.ToString("N")
            };

            var response = await _httpClient.PostAsJsonAsync(apiUrl.Trim(), payload);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"algorithm returned non-2xx response: {(int)response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<DetectionResult>()
                ?? throw new InvalidOperationException("algorithm response parse failed");
        }
    }
}
