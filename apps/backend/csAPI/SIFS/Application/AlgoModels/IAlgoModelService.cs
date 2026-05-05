using SIFS.Shared.Results;

namespace SIFS.Application.AlgoModels
{
    public interface IAlgoModelService
    {
        Task<Result<AlgoModelDto>> CreateAlgoAsync(AlgoModelCreateDto dto, Guid actorId);
        Task<Result<AlgoModelDto>> UpdateAlgoAsync(int id, AlgoModelUpdateDto dto, Guid actorId);
        Task<Result<AlgoModelDto>> EnableAlgoAsync(int id, Guid actorId);
        Task<Result<AlgoModelDto>> DisableAlgoAsync(int id, Guid actorId);
        Task<Result<AlgoModelDto>> GetAlgoAsync(int id);
        Task<Result<Paged<AlgoModelDto>>> ListAlgosAsync(AlgoModelQuery query);
        Task<Result<AlgoModelDto>> GetEnabledAlgoByIdAsync(int id);
        Task<Result> SoftDeleteAlgoAsync(int id, Guid actorId);
    }
}
