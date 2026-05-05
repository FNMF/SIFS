using SIFS.Application.AlgoModels;

namespace SIFS.Infrastructure.External
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAlgoModelService _algoModelService;

        public AiService(HttpClient httpClient, IAlgoModelService algoModelService)
        {
            _httpClient = httpClient;
            _algoModelService = algoModelService;
        }

        public async Task<DetectionResult> DetectAsync(AiServiceType type, string imageUrl, int? level)
        {
            var algoResult = await _algoModelService.GetEnabledAlgoByIdAsync((int)type);
            if (!algoResult.IsSuccess)
                throw new InvalidOperationException(algoResult.Message);

            var payload = new
            {
                image_url = imageUrl,
                level = level
            };

            var response = await _httpClient.PostAsJsonAsync(algoResult.Data.ApiUrl, payload);

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<DetectionResult>())!;
        }
    }
}
