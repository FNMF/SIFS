using SIFS.Shared.Results;

namespace SIFS.Infrastructure.External
{
    public interface IAlgorithmEndpointResolver
    {
        Task<Result<AlgorithmEndpointResolution>> ResolveAsync(AiServiceType type);
    }
}
