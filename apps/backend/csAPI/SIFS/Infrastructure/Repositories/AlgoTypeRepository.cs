using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class AlgoTypeRepository: IAlgoTypeRepository
    {
        private readonly SIFSContext _context;
        public AlgoTypeRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task<Result<List<AlgoType>>> GetAllAsync()
        {
            var types = await _context.AlgoTypes.ToListAsync();
            return Result<List<AlgoType>>.Success(types);
        }
    }
}
