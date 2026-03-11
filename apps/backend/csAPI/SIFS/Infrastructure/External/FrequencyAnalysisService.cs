using Microsoft.Extensions.Options;

namespace SIFS.Infrastructure.External
{
    public class FrequencyAnalysisService : IAiService
    {
        private readonly HttpClient _httpClient;


        private readonly AiServiceOptions _options;

        public FrequencyAnalysisService(HttpClient httpClient, IOptions<AiServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        public AiServiceType Type => AiServiceType.FrequencyAnalyzer;

        public async Task<DetectionResult> DetectAsync(string imagePath)
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(
                await File.ReadAllBytesAsync(imagePath));

            form.Add(fileContent, "file", Path.GetFileName(imagePath));

            var response = await _httpClient.PostAsync(
                _options.FrequencyAnalyzer, form);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<DetectionResult>();

            return result!;
        }
    }
}
