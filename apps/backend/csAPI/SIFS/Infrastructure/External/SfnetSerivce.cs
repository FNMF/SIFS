using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;

namespace SIFS.Infrastructure.External
{
    public class SfnetSerivce : IAiService
    {
        private readonly HttpClient _httpClient;


        private readonly AiServiceOptions _options;

        public SfnetSerivce(HttpClient httpClient, IOptions<AiServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        public AiServiceType Type => AiServiceType.Sfnet;

        public async Task<DetectionResult> DetectAsync(string imagePath)
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(
                await File.ReadAllBytesAsync(imagePath));

            form.Add(fileContent, "file", Path.GetFileName(imagePath));

            var response = await _httpClient.PostAsync(
                _options.Sfnet, form);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<DetectionResult>();

            return result!;
        }
    }
}
