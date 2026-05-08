using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Application.Scheduling
{
    public interface IAlgoRuntimeConfigResolver
    {
        AlgoRuntimeConfig Resolve(AlgoModel algoModel);
    }
}
