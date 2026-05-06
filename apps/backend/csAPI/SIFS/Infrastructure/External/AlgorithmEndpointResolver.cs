using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.External
{
    public class AlgorithmEndpointResolver : IAlgorithmEndpointResolver
    {
        private readonly IAlgoModelRepository _algoModelRepository;

        public AlgorithmEndpointResolver(IAlgoModelRepository algoModelRepository)
        {
            _algoModelRepository = algoModelRepository;
        }

        public async Task<Result<AlgorithmEndpointResolution>> ResolveAsync(AiServiceType type)
        {
            var algoId = (int)type;
            var algoName = type.ToString();

            var modelById = await _algoModelRepository.FindByIdAsync(algoId);
            if (modelById != null)
                return ValidateModel(modelById);

            var modelByName = await _algoModelRepository.FindByNameAsync(algoName);
            if (modelByName != null)
                return ValidateModel(modelByName);

            return Result<AlgorithmEndpointResolution>.Fail(ResultCode.NotFound, $"ALGORITHM_NOT_FOUND: {algoName}");
        }

        private static Result<AlgorithmEndpointResolution> ValidateModel(Persistence.Models.AlgoModel model)
        {
            if (!model.Enabled)
                return Result<AlgorithmEndpointResolution>.Fail(ResultCode.Forbidden, $"ALGORITHM_DISABLED: {model.Name}");

            if (string.IsNullOrWhiteSpace(model.ApiUrl))
                return Result<AlgorithmEndpointResolution>.Fail(ResultCode.InvalidInput, $"ALGORITHM_API_URL_EMPTY: {model.Name}");

            return Result<AlgorithmEndpointResolution>.Success(new AlgorithmEndpointResolution
            {
                AlgoModelId = model.Id,
                AlgoName = model.Name,
                ApiUrl = model.ApiUrl.Trim()
            });
        }
    }
}
