using SIFS.Application.AlgoModels;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoInfoService : IAlgoInfoService
    {
        private readonly IAlgoModelRepository _algoModelRepository;

        public AlgoInfoService(IAlgoModelRepository algoModelRepository)
        {
            _algoModelRepository = algoModelRepository;
        }

        public async Task<Result<List<AlgoModelDto>>> GetAllAsync()
        {
            var page = await _algoModelRepository.ListAsync(new AlgoModelQuery
            {
                Enabled = true,
                Page = 1,
                PageSize = 100
            });

            return Result<List<AlgoModelDto>>.Success(page.Data);
        }
    }
}
