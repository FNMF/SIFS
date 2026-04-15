using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoInfoService: IAlgoInfoService
    {
        private readonly IAlgoTypeRepository _algoTypeRepository;
        public AlgoInfoService(IAlgoTypeRepository algoTypeRepository)
        {
            _algoTypeRepository = algoTypeRepository;
        }
        public async Task<Result<List<AlgoType>>> GetAllAsync()
        {
            return await _algoTypeRepository.GetAllAsync();
        }
    }
}
