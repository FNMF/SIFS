using SIFS.Shared.Results;

namespace SIFS.Infrastructure.External
{
    public interface IAlgorithmEndpointResolver
    {
        Task<Result<AlgorithmEndpointResolution>> ResolveByIdAsync(int algoModelId);
        Task<Result<AlgorithmEndpointResolution>> ResolveByNameAsync(string algoName);
        Task<Result<AlgorithmEndpointResolution>> ResolveAsync(int? algoModelId, string? algoName);
    }
}
