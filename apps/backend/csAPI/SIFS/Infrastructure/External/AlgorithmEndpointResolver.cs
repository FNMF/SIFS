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

        public async Task<Result<AlgorithmEndpointResolution>> ResolveByIdAsync(int algoModelId)
        {
            var model = await _algoModelRepository.FindByIdAsync(algoModelId);
            return model == null
                ? Result<AlgorithmEndpointResolution>.Fail(ResultCode.NotFound, $"ALGORITHM_NOT_FOUND: {algoModelId}")
                : ValidateModel(model);
        }

        public async Task<Result<AlgorithmEndpointResolution>> ResolveByNameAsync(string algoName)
        {
            if (string.IsNullOrWhiteSpace(algoName))
                return Result<AlgorithmEndpointResolution>.Fail(ResultCode.InvalidInput, "ALGORITHM_NAME_EMPTY");

            var model = await _algoModelRepository.FindByNameAsync(algoName.Trim());
            return model == null
                ? Result<AlgorithmEndpointResolution>.Fail(ResultCode.NotFound, $"ALGORITHM_NOT_FOUND: {algoName}")
                : ValidateModel(model);
        }

        public async Task<Result<AlgorithmEndpointResolution>> ResolveAsync(int? algoModelId, string? algoName)
        {
            if (algoModelId.HasValue)
                return await ResolveByIdAsync(algoModelId.Value);

            if (!string.IsNullOrWhiteSpace(algoName))
                return await ResolveByNameAsync(algoName);

            return Result<AlgorithmEndpointResolution>.Fail(ResultCode.InvalidInput, "ALGORITHM_NOT_SPECIFIED");
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
